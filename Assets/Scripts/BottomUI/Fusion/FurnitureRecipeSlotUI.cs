// FurnitureRecipeSlotUI.cs  (도안 목록의 칸 프리팹에 붙임. 같은 오브젝트에 Button 필요)
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FurnitureRecipeSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public GameObject selectedMark;    // 선택됐을 때 켜질 테두리 (선택 사항)
    public GameObject craftableMark;   // 지금 제작 가능할 때 켜질 표시 (선택 사항)

    public FurnitureRecipe Recipe { get; private set; }

    public void Setup(FurnitureRecipe recipe, Action<FurnitureRecipe> onClick)
    {
        Recipe = recipe;

        if (icon != null)
        {
            icon.sprite = recipe.ResultIcon;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
        }
        if (nameText != null) nameText.text = recipe.DisplayName;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke(recipe));

        SetState(false, false);
    }

    public void SetState(bool selected, bool craftable)
    {
        if (selectedMark != null) selectedMark.SetActive(selected);
        if (craftableMark != null) craftableMark.SetActive(craftable);
    }
}