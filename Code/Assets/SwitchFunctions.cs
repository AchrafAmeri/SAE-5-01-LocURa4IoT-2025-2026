using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SwitchFunctions : MonoBehaviour
{
    [SerializeField] Button ButtonUp;
    [SerializeField] Button ButtonDown;
    [SerializeField] TogleAnimation Table;
    [SerializeField] TogleAnimation Placard;
    private IEnumerator DelayCoroutine;

    private void Start()
    {
            ButtonUp.interactable = !(Table.getIsgoingUp() || Placard.getIsgoingUp());
            ButtonDown.interactable = Table.getIsgoingUp() || Placard.getIsgoingUp();
    }
    public void Switch()
    {
        if (!(Table.getIsgoingUp() || Placard.getIsgoingUp()))
        {
            Placard.Toggle();
            if (DelayCoroutine != null)
            {
                StopCoroutine(DelayCoroutine);
            }
            DelayCoroutine = SwitchAsync(Table);
            StartCoroutine(DelayCoroutine);
        }
        else
        {
            Table.Toggle();
            if (DelayCoroutine != null)
            {
                StopCoroutine(DelayCoroutine);
            }
            DelayCoroutine = SwitchAsync(Placard);
            StartCoroutine(DelayCoroutine);
        }
    }

    private IEnumerator SwitchAsync(TogleAnimation DelayedFurniture)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (DelayedFurniture.getIsgoingUp())
        {
            ButtonUp.interactable = true;
            ButtonDown.interactable = false;
        }
        else
        {
            ButtonUp.interactable = false;
            ButtonDown.interactable = true;
        }

        DelayedFurniture.Toggle();
    }
}
