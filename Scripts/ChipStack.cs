
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class ChipStack : UdonSharpBehaviour
{
    public int chipValue;
    public int chipCount = 5;

    [Range(0, 50)]
    public int maxChipCount = 20;

    [Range(0f, 0.2f)]
    public float chipSpacing = 0.005f;

    public GameObject TemplateChip;
    public VRCObjectPool chipPool;

    private void OnValidate()
    {
        chipPool.Pool = new GameObject[maxChipCount];
    }

    private void Awake()
    {

        // for (int i = 0; i < chipPool.Pool.Length; i++)
        // {
        //     GameObject newChip = (GameObject)Instantiate(TemplateChip);
        //     newChip.SetActive(false);
        //     newChip.name = "Chip " + i;
        //     newChip.transform.SetParent(chipPool.transform);
        //     chipPool.Pool[i] = newChip;
        // }

        for (int i = 0; i < chipCount; i++)
        {
            GameObject chip = chipPool.TryToSpawn();
            chip.SetActive(true);
            chip.name = "Cool Chip " + i;
            float chipHeight = chip.GetComponent<Renderer>().bounds.size.y;
            chip.transform.position = TemplateChip.transform.position +
                                        new Vector3(0, i * (chipHeight + chipSpacing), 0);
            Debug.Log($"{chip.name} position {chip.transform.position}, height {chipHeight}");
        }

        TemplateChip.SetActive(false);
    }

    public int TotalValue()
    {
        return chipValue * chipCount;
    }
}
