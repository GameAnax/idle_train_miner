using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoggyUnlockScreen : MonoBehaviour
{
    public GameObject contentObj;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI previewsLevelDamageText;
    public TextMeshProUGUI currentLevelDamageText;
    public Button greatButton;
    public Image rayImage;
    public Image titleImage;
    public Image boggyIconImage;

    public Action greatButtonAction;

    [SerializeField]
    private RectTransform rayRectTransform;
    [SerializeField]
    private RectTransform titleRectTransform;
    [SerializeField]
    private RectTransform greatButtonRectTransform;
    [SerializeField]
    private ParticleSystem confettiParticle;
    private Sequence _entranceSequence;
    private Tween _rotateTween;
    private Tween _buttonTween;

    void Start()
    {
        greatButton.onClick.AddListener(OnClickGreatButton);



    }
    void OnDisable()
    {
        _entranceSequence?.Kill();
        _rotateTween?.Kill();
        _buttonTween?.Kill();
    }



    public void ShowScreen()
    {
        GameManager.instance.storageBoggyWorldCameraObj.SetActive(false);
        gameObject.SetActive(true);
        PlayRayAnimation();
        ButtonAnimation();
        TitleTextAnimation();
        confettiParticle.Play();
    }
    public void SetUpData(int currentBoggyLevel)
    {
        int totalLength = GameManager.instance.boggyConfigs.Count;
        int upgradeBoggyLevel = currentBoggyLevel + 1;
        upgradeBoggyLevel = Mathf.Min(upgradeBoggyLevel, totalLength - 1);

        var currentBoggyConfig = GameManager.instance.boggyConfigs[currentBoggyLevel];
        var upgradeBoggyData = GameManager.instance.boggyConfigs[upgradeBoggyLevel];

        levelText.text = $"Level {upgradeBoggyData.boggyLevel}";
        previewsLevelDamageText.text = $"{currentBoggyConfig.boggyDamage}";
        currentLevelDamageText.text = $"{upgradeBoggyData.boggyDamage}";
        boggyIconImage.sprite = upgradeBoggyData.boggySprite;
    }
    public void HideScreen()
    {
        gameObject.SetActive(false);
        GameManager.instance.storageBoggyWorldCameraObj.SetActive(true);
    }



    private void PlayRayAnimation()
    {
        rayRectTransform.localScale = Vector3.zero;
        rayImage.color = new Color(rayImage.color.r, rayImage.color.g, rayImage.color.b, 0);

        _entranceSequence = DOTween.Sequence();
        _entranceSequence.Join(rayRectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        _entranceSequence.Join(rayImage.DOFade(1f, 0.5f));

        _rotateTween = rayRectTransform.DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    private void ButtonAnimation()
    {
        greatButtonRectTransform.localScale = Vector3.one;
        _buttonTween = greatButtonRectTransform.DOScale(1.05f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    private void TitleTextAnimation()
    {
        titleRectTransform.anchoredPosition = new Vector2(1.6f, 0);
        titleRectTransform.DOAnchorPos(new Vector2(1.6f, 641), 0.6f).SetEase(Ease.OutCubic);
    }



    private void OnClickGreatButton()
    {
        greatButtonAction?.Invoke();
        HideScreen();
    }
}
