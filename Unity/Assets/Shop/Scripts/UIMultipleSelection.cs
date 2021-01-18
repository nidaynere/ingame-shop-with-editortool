using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class UIMultipleSelection : MonoBehaviour
{
    [System.Serializable]
    public class IntArrayEvent : UnityEvent<List<int>> { }

    public IntArrayEvent OnValueChanged;

    [SerializeField] private bool initializeAtStart = true;

    List<int> currentSelection = new List<int>();

    private void Start()
    {
        if (initializeAtStart)
        Initialize();
    }

    public void Initialize ()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int index = i;
            transform.GetChild(i).GetComponentInChildren<Button>().onClick.AddListener(() => {
                Select(index);
                OnValueChanged.Invoke(currentSelection);
            });
        }
    }

    public void SelectAll()
    {
        currentSelection.Clear();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                Select(i);
            }
        }

        OnValueChanged.Invoke(currentSelection);
    }

    private void Select(int i)
    {
        bool activated = true;
        if (currentSelection.Contains(i))
        {
            activated = false;
            currentSelection.Remove(i);
        }
        else currentSelection.Add(i);

        var targetSelection = transform.GetChild(i);
        if (targetSelection != null)
        {
            targetSelection.Find("Active").gameObject.SetActive(activated);
        }
    }
}
