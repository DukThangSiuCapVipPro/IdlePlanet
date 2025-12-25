using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    [Header("Camera References")]
    public Camera mainCamera;
    [Range(6f, 7f)] public float cameraSizeActive = 6f;
    [Range(4f, 5f)] public float cameraSizeInactive = 5f;
    public float cameraPosYActive = -3.25f;
    public float cameraPosYInactive = 0f;
    [Header("UI References")]
    public RectTransform rectUpgradePanel;
    public SmoothButton buttonSwitchUpgradePanel;
    public ScrollUpgradeAdapter scrollCreaturesAdapter;
    public ItemCreature itemCreaturePrefab;
    public SmoothButton buttonSettings;
    public SmoothButton buttonShop;
    public TMP_Text txtStardust;

    [Header("Object References")]
    public Planet planet;

    protected override void Awake()
    {
        base.Awake();

        buttonSwitchUpgradePanel.onClick.RemoveAllListeners();
        buttonSwitchUpgradePanel.onClick.AddListener(OnClickSwitchUpgradePanel);

        DataManager.Instance.SubcribeOnPlayerDataChanged(OnPlayerDataChanged);
    }

    private void Start()
    {
        StartCoroutine(LoadUIs());
        StartCoroutine(LoadAssets());
    }

    void OnPlayerDataChanged(PlayerData playerData)
    {
        txtStardust.text = playerData.starDust.ToString();
    }

    void OnClickSwitchUpgradePanel()
    {
        bool isActive = rectUpgradePanel.anchoredPosition.y >= -200;
        isActive = !isActive;
        if (isActive)
        {
            rectUpgradePanel.DOAnchorPosY(-200, 0.5f).SetEase(Ease.OutBack);
            buttonSwitchUpgradePanel.transform.DORotate(new Vector3(0, 0, 0), 0f);
            mainCamera.DOOrthoSize(cameraSizeActive, 0.5f).SetEase(Ease.Linear);
            mainCamera.transform.DOMoveY(cameraPosYActive, 0.5f).SetEase(Ease.Linear);
        }
        else
        {
            rectUpgradePanel.DOAnchorPosY(-1100, 0.5f).SetEase(Ease.OutBack);
            buttonSwitchUpgradePanel.transform.DORotate(new Vector3(0, 0, 180), 0f);
            mainCamera.DOOrthoSize(cameraSizeInactive, 0.5f).SetEase(Ease.Linear);
            mainCamera.transform.DOMoveY(cameraPosYInactive, 0.5f).SetEase(Ease.Linear);
        }
    }

    IEnumerator LoadAssets()
    {
        DataManager dataManager = DataManager.Instance;
        foreach (var creature in dataManager.GetAllCreatures())
        {
            if (dataManager.GetOwnedItemLevel(creature.id) > 0)
            {
                yield return StartCoroutine(SpawnCreature(creature));
            }
        }
        yield return new WaitForSeconds(1.5f);
        LoadingScreenManager.Instance.HideLoadingScreen();
    }
    IEnumerator SpawnCreature(CreatureData creatureData)
    {
        GameObject creatureObj = Instantiate(creatureData.prefab);
        creatureObj.transform.SetParent(planet.spawnPointsContainer);
        creatureObj.transform.localPosition = planet.spawnPointsContainer.GetChild(creatureData.id).localPosition;
        creatureObj.transform.localScale = Vector3.one;

        yield return null;
    }
    IEnumerator LoadUIs()
    {
        DataManager dataManager = DataManager.Instance;
        scrollCreaturesAdapter.SetItems(dataManager.GetAllCreatures());
        yield return null;
    }
}
