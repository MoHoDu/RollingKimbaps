#if UNITY_EDITOR
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools; // 에디터 테스트
using System.IO;
using System.Linq;

public class SheetToSODataImporterTests
{
    [UnityTest]
    public IEnumerator DownloadAllAndGenerateFromUrl_CreatesSOAssetsForAllSheetTypes()
    {
        // 준비: 테스트용 임포터 인스턴스 생성
        var importerWindow = ScriptableObject.CreateInstance<SheetToSODataImporter>();

        // output 폴더가 이미 있다면 삭제 (클린업)
        string outputFolder = "Assets/Resources/DataObjects";
        if (Directory.Exists(outputFolder))
            Directory.Delete(outputFolder, true);

        // EditorCoroutine 직접 실행
        IEnumerator coroutine = typeof(SheetToSODataImporter)
            .GetMethod("DownloadAllAndGenerateFromUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(importerWindow, null) as IEnumerator;

        // 코루틴을 직접 하나씩 순회
        while (coroutine.MoveNext())
        {
            yield return coroutine.Current;
        }

        AssetDatabase.Refresh();

        // 검증: 각 SheetType별로 폴더와 asset이 실제 생성되었는지 체크
        foreach (SheetType type in System.Enum.GetValues(typeof(SheetType)))
        {
            if (type == SheetType.All) continue;

            string typeFolder = Path.Combine(outputFolder, type.ToString());
            Assert.IsTrue(Directory.Exists(typeFolder), $"폴더 {typeFolder}가 생성되어야 합니다.");

            // 하나 이상의 .asset 파일이 생성됐는지 확인
            string[] assets = Directory.GetFiles(typeFolder, "*.asset");
            Assert.IsTrue(assets.Length > 0, $"{type} 시트에 대한 ScriptableObject가 하나 이상 생성되어야 합니다.");
        }
    }
}
#endif
