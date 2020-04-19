using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectSpeed : MonoBehaviour
{
    public delegate void ChangeSpeedEvent(int speed);
    public static event ChangeSpeedEvent OnChangeSpeed;

    [SerializeField] int speed = 96;
    [SerializeField] bool selectOnEnable = false;
    [SerializeField] Sprite[] sprites;

    static List<UISelectSpeed> _sisters;

    static List<UISelectSpeed> sisters
    {
        get
        {
            if (_sisters == null)
            {
                _sisters = new List<UISelectSpeed>();
                _sisters.AddRange(FindObjectsOfType<UISelectSpeed>());
            }
            return _sisters;
        }
    }

    private void OnEnable()
    {
        if (selectOnEnable) HandleClick();
        if (!sisters.Contains(this)) sisters.Add(this);
    }

    private void OnDisable()
    {
        if (sisters.Contains(this)) sisters.Remove(this);
    }

    public void HandleClick()
    {
        OnChangeSpeed?.Invoke(speed);
        var sisters = UISelectSpeed.sisters;
        for (int i=0, l=sisters.Count; i<l ; i++)
        {
            if (sisters[i] == this) continue;
            sisters[i].SetNotActive();
        }
        GetComponent<Image>().sprite = sprites[1];
    }

    void SetNotActive()
    {
        GetComponent<Image>().sprite = sprites[0];
    }
}
