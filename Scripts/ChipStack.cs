
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class ChipStack : UdonSharpBehaviour
{
    public int chipValue;
    public int chipCount = 5;
    [Range(0f, 0.2f)]
    public float chipSpacing = 0.005f;
    public GameObject TemplateChip;
    // public VRCObjectPool chipPool;

    private void OnValidate()
    {

    }

    private void Awake()
    {
        StackChips();

        TemplateChip.SetActive(false);
    }

    private void StackChips()
    {
        for (int i = 0; i < chipCount; i++)
        {
            GameObject chip = Instantiate(TemplateChip);
            chip.name = "Cool Chip " + i;
            chip.transform.SetParent(this.transform);
            float chipHeight = chip.GetComponent<Renderer>().bounds.size.y;
            chip.transform.position = TemplateChip.transform.position +
                                        new Vector3(0, i * (chipHeight + chipSpacing), 0);
            Debug.Log($"Chip {i} position {chip.transform.position}, height {chipHeight}");
        }
    }

    public int TotalValue()
    {
        return chipValue * chipCount;
    }
}
