using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public bool repRect = true;
    public bool repSphere = false;
    public bool errorRangingParRapportNodeRéel = true;
    public bool errorRangingParRapportNodeEstim = false;
    public List<Color> ShereColor;
    private float alpha = 0.2f;


    private void Start()
    {
        ShereColor.Add(new Color(0, 0, 1, alpha));
        ShereColor.Add(new Color(1, 0, 0, alpha));
        ShereColor.Add(new Color(0, 1, 0, alpha));
        ShereColor.Add(new Color(1, 1, 0, alpha));
        ShereColor.Add(new Color(0, 1, 1, alpha));
        ShereColor.Add(new Color(1, 0, 1, alpha));
        ShereColor.Add(new Color(0.5f, 0.5f, 0.5f, alpha));
        ShereColor.Add(new Color(1, 0.5f, 0, alpha));
        ShereColor.Add(new Color(1, 0, 0.5f, alpha));
        ShereColor.Add(new Color(0.5f, 0, 1, alpha));
        ShereColor.Add(new Color(0, 0.5f, 1, alpha));
        ShereColor.Add(new Color(0.5f, 1, 0, alpha));
        ShereColor.Add(new Color(0, 1, 0.5f, alpha));
        ShereColor.Add(new Color(1, 0.5f, 0.5f, alpha));
        ShereColor.Add(new Color(0.5f, 1, 0.5f, alpha));
        ShereColor.Add(new Color(0.5f, 0.5f, 1, alpha));

    }

    public void DrawEstimationLine(GameObject real, GameObject estimated, LineRenderer Estimation)
    {
        Estimation.SetPosition(0, real.transform.position);
        Estimation.SetPosition(1, estimated.transform.position);
    }

    public void DrawRangingLine(GameObject anchor, GameObject node, LineRenderer lineCommunication, LineRenderer lineError, float distance, float range)
    {
        Vector3 errorRangingPoint = RangingErrorPoint(anchor, node, distance, range);
        lineCommunication.SetPosition(0, anchor.transform.position);
        lineCommunication.SetPosition(1, errorRangingPoint);

        lineError.SetPosition(0, errorRangingPoint);
        lineError.SetPosition(1, node.transform.position);
    }

    public void DrawSphereLine(GameObject anchor, GameObject node, LineRenderer lineCommunication, float distance, float range)
    {
        Vector3 errorRangingPoint = RangingErrorPoint(anchor, node, distance, range);
        lineCommunication.SetPosition(0, anchor.transform.position);
        lineCommunication.SetPosition(1, errorRangingPoint);
    }

    private Vector3 RangingErrorPoint (GameObject A, GameObject R, float d,  float r)
    {
        float t;
        Vector3 E = new Vector3();
        float xA = A.transform.position.x;
        float yA = A.transform.position.y;
        float zA = A.transform.position.z;

        float xR = R.transform.position.x;
        float yR = R.transform.position.y;
        float zR = R.transform.position.z;

        t = ((d * r)
            /(Mathf.Pow(xR-xA,2) + Mathf.Pow(yR - yA, 2) + Mathf.Pow(zR - zA, 2)));

        E.x = xA + (xR - xA) * t;
        E.y = yA + (yR - yA) * t;
        E.z = zA + (zR - zA) * t;

        return E;
    }

    public void SetErrorRealNode()
    {
        errorRangingParRapportNodeRéel = true;
        errorRangingParRapportNodeEstim = false;
    }

    public void SetErrorEstimNode()
    {
        errorRangingParRapportNodeRéel = false;
        errorRangingParRapportNodeEstim = true;
    }

    public void SetRepRect()
    {
        repRect = true;
        repSphere = false;

    }

    public void SetRepSphere()
    {
        repRect = false;
        repSphere = true;

    }
}
