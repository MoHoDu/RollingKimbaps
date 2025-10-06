using UIs.Base;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelUI : CanvasUI
{
    [SerializeField] private GameObject _bg;
    [SerializeField] private TutorialChapterUI[] _chapters;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _previousButton;
    [SerializeField] private Button _exitButton;

    private int _index;

    protected override void Initialize()
    {
        base.Initialize();

        _exitButton?.onClick.AddListener(CloseTutorial);
        _nextButton?.onClick.AddListener(OnNext);
        _previousButton?.onClick.AddListener(OnPrevious);

        InitValues();
        OnNext();
    }

    public void OpenTutorial()
    {
        _bg?.SetActive(true);
        InitValues();
        OnNext();
    }

    public void CloseTutorial()
    {
        _bg?.SetActive(false);
    }

    public void InitValues()
    {
        _index = -1;
    }

    public void OnNext()
    {
        _index++;
        if (_index >= _chapters.Length)
        {
            InitValues();
            CloseTutorial();
        }

        _previousButton.interactable = _index > 0;

        for (int i = 0; i < _chapters.Length; i++)
        {
            _chapters[i]?.ActiveOnOf(i == _index);
        }
    }

    public void OnPrevious()
    {
        _index--;
        _previousButton.interactable = _index > 0;

        for (int i = 0; i < _chapters.Length; i++)
        {
            _chapters[i]?.ActiveOnOf(i == _index);
        }
    }
}