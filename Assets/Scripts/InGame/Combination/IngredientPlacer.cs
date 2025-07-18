using System;
using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using InGame.PrapManagement.Praps;
using ManagerSystem;
using ManagerSystem.InGame;
using UIs;
using UnityEngine;
using Utils;

namespace InGame.Combination
{
    public class SpawnedIngredient
    {
        public EIngredientIndex index;
        public float xPos;
    }

    public class IngredientPlacer
    {
        // DI
        private CombinationManager _combinationManager;
        private PrapManager _prapManager;
        private RaceStatus _inRaceState;
        private CharacterHandler _player;

        // 계산을 위한 값들
        private List<SpawnedIngredient> _activeIngredients = new();     // 활성화 되어 있는 재료들
        private Dictionary<int, int[]> placedCountPerRecipe = new();    // 레시피 당 추가 스폰이 필요한 각 재료의 개수
        private float lastSpawnX = float.NegativeInfinity;              // 이전 스폰 X 위치

        // 동적 조절 가능한 생성 빈도 매개변수
        private float baseSpawnDistance = 5f;                          // 기본 거리 기준 간격
        private float baseGapTime = 0.7f;                               // 기본 플레이어 도달 시간
        private float minSpawnDistance = 3f;                            // 최소 스폰 거리
        private float maxSpawnDistance = 8f;                           // 최대 스폰 거리
        private float minGapTime = 0.3f;                                // 최소 갭 타임
        private float maxGapTime = 1.2f;                                // 최대 갭 타임
        private float currentSpawnDistance = 5f;                       // 현재 동적 조절된 스폰 거리
        private float currentGapTime = 0.7f;                            // 현재 동적 조절된 갭 타임

        // 기존 매개변수 (호환성 유지)
        private float spawnDistance => currentSpawnDistance;            // 동적 조절된 거리 기준 간격
        private float gapTime => currentGapTime;                        // 동적 조절된 플레이어 도달 시간
        private int floorCount = 2;                                     // 재료를 한 번에 생성할 최대 개수 (y로 나열하므로 층이라 표현)
        private int totalIngredientCount = 3;                           // 총 재료의 개수
        private float floorHeight = 8f;                                 // 층 간 높이
        private int requiredMinCount = 2;                               // 레시피 당 재료 최소 생성 개수

        public void Initialize(CombinationManager combinationManager, PrapManager prapManager, RaceStatus raceStatus)
        {
            _combinationManager = combinationManager;
            _prapManager = prapManager;
            _inRaceState = raceStatus;
            lastSpawnX = float.NegativeInfinity;
            totalIngredientCount = Enum.GetValues(typeof(EIngredientIndex)).Length;

            // 서빙 성공 이벤트 연결
            SubscribeToServingEvents();
        }

        public void SetHandler(CharacterHandler player)
        {
            _player = player;
        }

        public void CheckOrderAndPlaceIngredients(List<OrderData> orderList)
        {
            if (orderList == null || orderList.Count == 0) return;

            TrySpawnIngredient(orderList);
        }

        /// <summary>
        /// 재료 스폰 시도
        /// </summary>
        /// <param name="activeRecipes">현재 활성화 된 오더 리스트</param>
        private void TrySpawnIngredient(List<OrderData> activeRecipes)
        {
            // 필요한 정보 색인
            float playerSpeed = _inRaceState.Velocity;
            float playerPosX = _inRaceState.TravelDistance;

            // 동적 생성 빈도 조절
            uint collectedMask = _combinationManager.GetCollectedMask();
            UpdateSpawnFrequency(activeRecipes, collectedMask);

            // ① Lookahead 계산: 플레이어가 gapTime 만큼 시간이 지난 후의 위치부터 배치 
            float lookaheadDistance = playerSpeed * gapTime;
            float baseX = playerPosX + lookaheadDistance;

            // ② 1초 동안 가능한 슬롯 수 계산: 현재 속도 기준으로 스폰 간격을 나눠 생산 가능 슬롯을 구함
            // 슬롯 = 배치 가능 범위(슬롯에 재료를 1개(1열 1개 제한)까지 생성 가능)
            float spawnTime = spawnDistance / playerSpeed;
            int spawnSlotsThisSecond = (int)Mathf.Floor(1.0f / spawnTime);
            if (spawnSlotsThisSecond < 1) spawnSlotsThisSecond = 1;

            spawnSlotsThisSecond = Mathf.Min(spawnSlotsThisSecond, 5); // 최대 5개 슬롯

            // ③ 각 슬롯 별로 배치
            for (int slot = 0; slot < spawnSlotsThisSecond; slot++)
            {
                // (A) “slot만큼 떨어진 가상 위치” 계산
                float tentativeX = baseX + spawnDistance * slot;

                // (B) “마지막에 뿌린 지점 + spawnDistance” 보정
                // : 배치 첫 슬롯은 이전 생성 위치와 최소 생성 간격보다 좁을 수도 있기 때문 
                float minAllowedX = lastSpawnX + spawnDistance;
                float curX = Mathf.Max(tentativeX, minAllowedX);

                // (C) 장애물/층 체크
                List<int> availableLayers = GetAvailableYLayers(curX);
                if (availableLayers.Count == 0)
                {
                    // 만약 이 위치에 아이템을 놓을 수 없다면
                    // “lastSpawnX”를 갱신하지 않고, 다음 슬롯(slot+1)로 넘어가서 재계산
                    continue;
                }

                // (D) 활성 레시피 거리 기준 만료 검사
                for (int i = activeRecipes.Count - 1; i >= 0; i--)
                {
                    var r = activeRecipes[i];
                    if (playerPosX >= r.EndX)
                    {
                        activeRecipes.RemoveAt(i);
                    }
                }

                if (activeRecipes.Count == 0) return;

                // (E) 아이템 선택 로직 (HUGE_BONUS 포함)
                List<int> chosen = ChooseIngredientsToSpawn(curX, availableLayers.Count, collectedMask, activeRecipes);
                if (chosen.Count == 0)
                {
                    // 뽑을 재료가 없다면, 다음 슬롯으로
                    continue;
                }

                // (F) 실제 배치: 최대 availableLayers.Count만큼
                for (int j = 0; j < chosen.Count && j < availableLayers.Count; j++)
                {
                    int ingIdx = chosen[j];
                    int layer = availableLayers[j];

                    // 재료를 배치
                    SpawnedIngredient spawnInfo = new SpawnedIngredient { index = (EIngredientIndex)ingIdx, xPos = curX };
                    PlaceIngredient(spawnInfo, curX, layer);

                    // 배치 후 상태 업데이트
                    foreach (var r in activeRecipes)
                    {
                        if ((r.Mask & (1u << ingIdx)) != 0)
                        {
                            int count = GetPlacedCountPerRecipe(int.Parse(r.recipe.id), ingIdx);
                            SetPlacedCountPerRecipe(int.Parse(r.recipe.id), ingIdx, count + 1);
                        }
                    }

                    _activeIngredients.Add(spawnInfo);

                    // **“curX에 실제로 아이템을 뿌린 시점”이므로**  
                    // **다음 번 슬롯을 위해 lastSpawnX 갱신**  
                    lastSpawnX = curX;
                }
            }
        }

        /// <summary>
        /// 스폰 할 재료를 고름
        /// </summary>
        /// <param name="curX">타겟 위치</param>
        /// <param name="maxCount">생성 개수</param>
        /// <param name="collectedMask">수집된 재료 비트마스크</param>
        /// <param name="activeRecipes">활성화 된 오더 리스트</param>
        /// <returns>생성 할 재료들 인덱스</returns>
        List<int> ChooseIngredientsToSpawn(float curX, int maxCount, uint collectedMask, List<OrderData> activeRecipes)
        {
            // 필요한 정보 색인
            float playerSpeed = _inRaceState.Velocity;

            // (1) spawnedMask 계산 (화면에 떠 있는, 아직 획득 안 된 재료 비트 OR)
            uint spawnedMask = 0;
            foreach (var si in _activeIngredients)
            {
                // 수집되면 사라지게 될 것이라 수집 여부 bool 값 제거
                // if (!si.isCollected) {
                spawnedMask |= 1u << (int)si.index;
                // }
            }

            // (2) unionRecipeMask, needCount, missingInAny 계산
            int N = totalIngredientCount;       // 웬만하면 contactIngredientCount번 만큼 재료를 마주칠 수 있도록
            uint unionRecipeMask = 0;           // 활성화 된 모든 오더 레시피의 재료 비트마스크
            int[] needCount = new int[N];       // 재료 별 생성 개수
            bool[] missingInAny = new bool[N];  // 생성 되지 않았거나 수집 되지 않았으나 생성이 필요한 각 재료의 개수
            for (int i = 0; i < N; i++)
            {
                needCount[i] = 0;
                missingInAny[i] = false;
            }
            foreach (var r in activeRecipes)
            {
                unionRecipeMask |= r.Mask;
                uint missingMaskForR = r.Mask & ~(collectedMask | spawnedMask);
                for (int i = 0; i < N; i++)
                {
                    uint bit = (1u << i);
                    if ((r.Mask & bit) != 0)
                    {
                        needCount[i]++;
                    }
                    if ((missingMaskForR & bit) != 0)
                    {
                        missingInAny[i] = true;
                    }
                }
            }

            // (3) 각 재료별 기본 점수 계산
            float[] score = new float[N];
            for (int i = 0; i < N; i++)
            {
                score[i] = float.NegativeInfinity;
                uint bit = (1u << i);
                // 생성이 필요하지 않다면 패스
                if ((unionRecipeMask & bit) == 0) continue;

                float s = 0f;
                // 생성이 필요하나 생성되지 않은 재료에게 100점 추가
                if (missingInAny[i])
                {
                    s += 100f;
                }
                // 필요한 개수만큼 10점씩 추가
                s += needCount[i] * 10f;
                // 수집된 재료라면 20점 감소
                if ((collectedMask & bit) != 0)
                {
                    s -= 20f;
                }

                // (4) “최소 1회 스폰 보장” (placedCountPerRecipe[r.Id][i] == 0)
                bool needFirst = false;
                foreach (var r in activeRecipes)
                {
                    // 생성이 필요하지만 레시피 당 단 한 번도 생성되지 않은 경우 (레시피 당 이므로 1 * 해당 재료가 필요한 레시피 개수)
                    if ((r.Mask & bit) != 0 && GetPlacedCountPerRecipe(int.Parse(r.recipe.id), i) == 0)
                    {
                        needFirst = true;
                        break;
                    }
                }
                // 레시피 당 첫 생성 보장 점수로 1000점 추가
                if (needFirst)
                {
                    s += 1000f;
                }

                // (5) “두 번째 이후 스폰 보장” 패널티:  
                float penalty = 0f;
                foreach (var r in activeRecipes)
                {
                    if ((r.Mask & bit) == 0) continue;

                    // 남은 시간 = (r.EndX – curX) / playerSpeed
                    float remainingDistanceToEnd = r.EndX - curX;                                           // 남은 거리 계산
                    if (remainingDistanceToEnd < 0f) continue;
                    float remainingTimeForRecipe = remainingDistanceToEnd / playerSpeed;                    // 남은 거리를 통과하는 시간

                    int placedSoFar = GetPlacedCountPerRecipe(int.Parse(r.recipe.id), i);        // 레시피 당 해당 재료의 생성 개수
                    int needMore = Math.Max(0, requiredMinCount - placedSoFar);                             // 남은 생성 개수
                    if (needMore > 0)
                    {
                        float spawnTime = spawnDistance / playerSpeed;                      // 스폰 장소를 지나는 시간
                        float neededTime = needMore * spawnTime;                            // 남은 개수만큼 생성한 범위를 지나는 시간 
                        // 만약 EndX까지 생성 필요 시간이 부족하다면 포기 (패널티 1000점 부여)
                        if (remainingTimeForRecipe < neededTime)
                        {
                            penalty += 1000f;
                        }
                        // 필요한 개수 * 5점만큼 패널티 부여 (빠르게 다른 재료를 늘려 해당 재료의 점수를 높임)
                        else
                        {
                            penalty += 5f * needMore;
                        }
                    }
                }
                s -= penalty;   // 패널티만큼 점수 감소
                score[i] = s;   // 점수 산정
            }

            // (4) 점수 내림차순 정렬 후 상위 maxCount개 인덱스 리턴
            List<int> idxs = new List<int>();
            for (int i = 0; i < N; i++)
            {
                // 점수가 부여된 항목만 가져옴
                if (score[i] > float.NegativeInfinity) idxs.Add(i);
            }
            // 점수가 많은 것부터 나열
            idxs.Sort((x, y) => score[y].CompareTo(score[x])); // Desc
            if (idxs.Count > maxCount)
            {
                idxs = idxs.GetRange(0, maxCount);  // 최상위 maxCount개 만큼만 생성
            }
            return idxs;
        }

        /// <summary>
        /// 실제로 월드에 재료 오브젝트를 배치
        /// </summary>
        /// <param name="spawnInfo">스폰 정보</param>
        /// <param name="posX">위치</param>
        /// <param name="floor">생성 층</param>
        private void PlaceIngredient(SpawnedIngredient spawnInfo, float posX, int floor)
        {
            // 재료 타입으로 랜덤한 재료 데이터 가져오기
            EIngredientIndex index = spawnInfo.index;
            List<IngredientData> ingredients = DataContainer.IngredientDatas.GetGroup(index);
            IngredientData ingredientData = ingredients.GetRandomElementsShuffle(1).FirstOrDefault();
            if (ingredientData == null) return;

            // IngredientData.prapID로 PrapData 가져오기
            PrapData data = DataContainer.Praps.Get(EPrapType.INGREDIENT, ingredientData.prapID);

            // 위치 계산
            Vector3 placedPosition = new Vector3(posX, floor * floorHeight, 0f);

            // PrapManager.Instance를 사용하여 Prap 생성
            Prap prap = _prapManager.CreatePrapInRealDistance(data, placedPosition, true);
            if (prap is IngredientPrap ingredientPrap)
            {
                prap.OnSpawned(ingredientData, spawnInfo);

                // OnTrigger에 _combinationManager.OnCollectIngredient() 연결 
                ingredientPrap.OnTriggerd += _combinationManager.OnCollectedIngredient;
                ingredientPrap.OnTriggerd += _player.OnCollectedIngredient;

                // OnDestroyed에 RemoveIngredient() 연결
                ingredientPrap.OnDestroyed += RemoveIngredient;
            }
            else
            {
                _prapManager.DestroyPrap(prap);
            }
        }

        private int GetPlacedCountPerRecipe(int recipeId, int ingredientIdx)
        {
            if (placedCountPerRecipe.TryGetValue(recipeId, out var ingredients))
            {
                return ingredients[ingredientIdx];
            }
            else
            {
                placedCountPerRecipe.Add(recipeId, new int[totalIngredientCount]);
                return 0;
            }
        }

        private void SetPlacedCountPerRecipe(int recipeId, int ingredientIdx, int count)
        {
            if (!placedCountPerRecipe.TryGetValue(recipeId, out _))
            {
                placedCountPerRecipe.Add(recipeId, new int[totalIngredientCount]);
            }

            placedCountPerRecipe[recipeId][ingredientIdx] = count;
        }

        /// <summary>
        /// 해당 X 좌표에서 장애물을 피해 생성 가능한 층 색인 (각 열별 최대 1개 재료 제한)
        /// </summary>
        /// <param name="posX">X 좌표</param>
        /// <returns>생성 가능한 층 중 1개만 선택하여 반환</returns>
        private List<int> GetAvailableYLayers(float posX)
        {
            Vector3 target = Vector3.right * posX;

            List<int> availableFloors = new();
            for (int floor = 0; floor < floorCount; floor++)
            {
                Vector3 curPosition = target + Vector3.up * (floor * floorHeight);
                if (_prapManager.CanCreateNewPrap(curPosition, EPrapType.INGREDIENT, true))
                {
                    availableFloors.Add(floor);
                }
            }

            // 각 열별 최대 1개 재료 생성 제한: 생성 가능한 층 중 1개만 선택
            List<int> selectedFloors = new();
            if (availableFloors.Count > 0)
            {
                // 랜덤하게 1개 층 선택
                int randomIndex = UnityEngine.Random.Range(0, availableFloors.Count);
                selectedFloors.Add(availableFloors[randomIndex]);
            }

            return selectedFloors;
        }

        private void RemoveIngredient(SpawnedIngredient ingredientData)
        {
            _activeIngredients.Remove(ingredientData);
        }

        /// <summary>
        /// 게임 상황에 따라 재료 생성 빈도를 동적으로 조절
        /// </summary>
        /// <param name="activeRecipes">활성화된 레시피 목록</param>
        /// <param name="collectedMask">수집된 재료 마스크</param>
        private void UpdateSpawnFrequency(List<OrderData> activeRecipes, uint collectedMask)
        {
            if (activeRecipes == null || activeRecipes.Count == 0)
            {
                // 활성 레시피가 없으면 기본값 사용
                currentSpawnDistance = baseSpawnDistance;
                currentGapTime = baseGapTime;
                return;
            }

            // 1. 부족한 재료 비율 계산
            float missingRatio = CalculateMissingIngredientRatio(activeRecipes, collectedMask);

            // 2. 활성 레시피 수에 따른 조절
            float recipeCountMultiplier = Mathf.Clamp(activeRecipes.Count / 3f, 0.5f, 2f);

            // 3. 1열 1개 제한에 따른 보상 (기존 대비 생성 기회 50% 감소를 보상)
            float oneColumnCompensation = 0.6f; // 40% 더 자주 생성

            // 4. 최종 조절 배수 계산
            float frequencyMultiplier = (1f + missingRatio) * recipeCountMultiplier * oneColumnCompensation;

            // 5. 동적 값 계산 및 제한
            currentSpawnDistance = Mathf.Clamp(baseSpawnDistance / frequencyMultiplier, minSpawnDistance, maxSpawnDistance);
            currentGapTime = Mathf.Clamp(baseGapTime / frequencyMultiplier, minGapTime, maxGapTime);
        }

        /// <summary>
        /// 현재 부족한 재료 비율 계산
        /// </summary>
        /// <param name="activeRecipes">활성화된 레시피 목록</param>
        /// <param name="collectedMask">수집된 재료 마스크</param>
        /// <returns>부족한 재료 비율 (0.0 ~ 1.0)</returns>
        private float CalculateMissingIngredientRatio(List<OrderData> activeRecipes, uint collectedMask)
        {
            uint spawnedMask = 0;
            foreach (var si in _activeIngredients)
            {
                spawnedMask |= (1u << (int)si.index);
            }

            int totalNeeded = 0;
            int totalMissing = 0;

            foreach (var recipe in activeRecipes)
            {
                uint availableMask = collectedMask | spawnedMask;
                uint missingMask = recipe.Mask & ~availableMask;

                // 각 재료별로 필요한 개수 계산
                for (int i = 0; i < totalIngredientCount; i++)
                {
                    uint bit = (1u << i);
                    if ((recipe.Mask & bit) != 0)
                    {
                        totalNeeded++;
                        if ((missingMask & bit) != 0)
                        {
                            totalMissing++;
                        }
                    }
                }
            }

            return totalNeeded > 0 ? (float)totalMissing / totalNeeded : 0f;
        }

        // 즉시 갱신을 위한 이벤트 연결
        public void SubscribeToServingEvents()
        {
            Cleanup();

            if (_combinationManager != null)
            {
                _combinationManager.onSuccessedServing += OnServingSuccess;
            }
        }

        public void UnsubscribeFromServingEvents()
        {
            if (_combinationManager != null)
            {
                _combinationManager.onSuccessedServing -= OnServingSuccess;
            }
        }

        /// <summary>
        /// 서빙 성공 시 즉시 재료 생성 갱신
        /// </summary>
        /// <param name="rewards">보상 정보</param>
        private void OnServingSuccess((int rewards, int tips) rewards)
        {
            // 레시피 완료 시 placedCountPerRecipe 초기화
            ClearPlacedCountForCompletedRecipes();

            // 즉시 재료 생성 상태 재계산
            ForceUpdateIngredientSpawning();
        }

        /// <summary>
        /// 완료된 레시피의 생성 카운트 초기화
        /// </summary>
        private void ClearPlacedCountForCompletedRecipes()
        {
            // 현재 활성화된 레시피 목록 가져오기
            var activeOrders = _combinationManager.Order.Orders;
            if (activeOrders == null || activeOrders.Count == 0)
            {
                // 활성 레시피가 없으면 모든 카운트 초기화
                placedCountPerRecipe.Clear();
                return;
            }

            // 활성화된 레시피 ID 목록 생성
            HashSet<int> activeRecipeIds = new HashSet<int>();
            foreach (var order in activeOrders)
            {
                activeRecipeIds.Add(int.Parse(order.recipe.id));
            }

            // 비활성화된 레시피의 카운트 제거
            var keysToRemove = new List<int>();
            foreach (var recipeId in placedCountPerRecipe.Keys)
            {
                if (!activeRecipeIds.Contains(recipeId))
                {
                    keysToRemove.Add(recipeId);
                }
            }

            foreach (var key in keysToRemove)
            {
                placedCountPerRecipe.Remove(key);
            }
        }

        /// <summary>
        /// 즉시 재료 생성 갱신 강제 실행
        /// </summary>
        private void ForceUpdateIngredientSpawning()
        {
            // 현재 활성화된 레시피 목록 가져오기
            var activeOrders = _combinationManager.Order.Orders;
            if (activeOrders == null || activeOrders.Count == 0) return;

            // 즉시 재료 생성 시도 (기존 로직 재사용)
            TrySpawnIngredient(activeOrders);
        }

        /// <summary>
        /// 리소스 정리 및 이벤트 해제
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromServingEvents();
        }
    }
}