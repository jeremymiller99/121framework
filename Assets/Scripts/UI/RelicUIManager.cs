using UnityEngine;
using System.Collections.Generic;

public class RelicUIManager : MonoBehaviour
{
    public GameObject relicUIPrefab;
    public PlayerController player;
    
    private List<GameObject> relicUIObjects = new List<GameObject>();

    void Start()
    {
        // Subscribe to relic events from the new system
        EventBus.Instance.OnWaveComplete += CheckForNewRelics;
    }

    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnWaveComplete -= CheckForNewRelics;
        }
    }

    void Update()
    {
        UpdateRelicUI();
    }

    void CheckForNewRelics(int waveNumber)
    {
        // This will be called when relics might be added
        UpdateRelicUI();
    }

    void UpdateRelicUI()
    {
        if (RelicManager.Instance == null) return;
        
        var playerRelics = RelicManager.Instance.GetPlayerRelics();
        
        // Remove excess UI objects
        while (relicUIObjects.Count > playerRelics.Count)
        {
            GameObject toRemove = relicUIObjects[relicUIObjects.Count - 1];
            relicUIObjects.RemoveAt(relicUIObjects.Count - 1);
            if (toRemove != null)
                Destroy(toRemove);
        }
        
        // Add missing UI objects
        while (relicUIObjects.Count < playerRelics.Count)
        {
            OnRelicPickup(relicUIObjects.Count);
        }
        
        // Update existing UI objects
        for (int i = 0; i < relicUIObjects.Count; i++)
        {
            if (relicUIObjects[i] != null)
            {
                RelicUI relicUI = relicUIObjects[i].GetComponent<RelicUI>();
                if (relicUI != null)
                {
                    relicUI.player = player;
                    relicUI.index = i;
                }
            }
        }
    }

    public void OnRelicPickup(int relicIndex)
    {
        // Make a new Relic UI representation
        GameObject rui = Instantiate(relicUIPrefab, transform);
        rui.transform.localPosition = new Vector3(-450 + 40 * relicIndex, 0, 0);
        
        RelicUI ruic = rui.GetComponent<RelicUI>();
        ruic.player = player;
        ruic.index = relicIndex;
        
        relicUIObjects.Add(rui);
    }
}
