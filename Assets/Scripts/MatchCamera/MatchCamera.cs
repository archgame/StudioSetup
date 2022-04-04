using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCamera : MonoBehaviour
{
    public UnityEngine.TextAsset csvFile = null;

    public Vector3[] _positions;
    public Vector3[] _forwards;
    public Vector3[] _ups;
    public float[] _time;

    // Start is called before the first frame update
    private void Start()
    {
        if (csvFile == null) { return; }
        ImportMatchCameraCSV(csvFile);
        //Debug.Break();
    }

    private float curTime = 0;

    // Update is called once per frame
    private void Update()
    {
        //guard statements
        if (csvFile == null) { return; }
        int pL = _positions.Length;
        int fL = _forwards.Length;
        int uL = _ups.Length;
        int tL = _time.Length;
        if (pL == 0 || fL == 0 || uL == 0 || tL == 0) { return; }
        if (pL != fL || uL != tL | pL != uL) { return; }

        //reset time
        if (Input.GetKeyDown(KeyCode.Space)) { curTime = 0; }

        //move camera based off current time
        for (int i = 1; i < _time.Length; i++)
        {
            float t1 = _time[i - 1];
            float t2 = _time[i];
            if (t1 <= curTime && curTime < t2) //if current time is between these two times
            {
                //Debug.Log("Time Found: " + curTime);
                float lerp = Remap(curTime, t1, t2, 0, 1);
                Vector3 position = Vector3.Lerp(_positions[i - 1], _positions[i], lerp);
                Vector3 forward = Vector3.Lerp(_forwards[i - 1], _forwards[i], lerp);
                Vector3 up = Vector3.Lerp(_ups[i - 1], _ups[i], lerp);
                this.transform.position = position; //set position
                this.transform.rotation = Quaternion.LookRotation(forward, up); //set rotation
                break;
            }
        }

        //increment Time
        curTime += (Time.deltaTime * 1000);
    }

    public float Remap(float x, float min, float max, float new_min, float new_max)
    {
        x = (x - min) * (new_max - new_min) / (max - min) + new_min;
        return x;
    }

    private void ImportMatchCameraCSV(TextAsset textAsset)
    {
        //make sure the text asset isn't empty
        if (textAsset == null) { return; }

        //get lines and make sure there are lines
        string text = textAsset.text;
        string[] lines = text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        if (lines.Length <= 1) { return; } //make sure there are lines

        //make sure all the expected headers exist
        //Layer, PositionX, PositionY, PositionZ, ForwardX, ForwardY, ForwardZ, NormalX, NormalY, NormalZ
        int d = -1;
        int time, posX, posY, posZ, fwdX, fwdY, fwdZ, upX, upY, upZ; //csv column indices
        time = posX = posY = posZ = fwdX = fwdY = fwdZ = upX = upY = upZ = d;
        string[] headers = lines[0].Split(',');
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i];
            //Debug.Log("headers: " + header);
            if (header == "Time") { time = i; }
            else if (header == "PositionX") { posX = i; }
            else if (header == "PositionY") { posY = i; }
            else if (header == "PositionZ") { posZ = i; }
            else if (header == "ForwardX") { fwdX = i; }
            else if (header == "ForwardY") { fwdY = i; }
            else if (header == "ForwardZ") { fwdZ = i; }
            else if (header == "NormalX") { upX = i; }
            else if (header == "NormalY") { upY = i; }
            else if (header == "NormalZ") { upZ = i; }
        }
        if (time == d || posX == d || posY == d || posZ == d || fwdX == d || fwdY == d || fwdZ == d || upX == d || upY == d || upZ == d)
        {
            string message = string.Format("Incorrect Headers:{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:"
              , time, posX, posY, posZ, fwdX, fwdY, fwdZ, upX, upY, upZ);
            Debug.Log(message); return;
        }

        //initialize array lengths
        _positions = new Vector3[lines.Length - 1];
        _forwards = new Vector3[lines.Length - 1];
        _ups = new Vector3[lines.Length - 1];
        _time = new float[lines.Length - 1];

        //loop through each line of the csv and add the information to the arrays
        for (int i = 1; i < lines.Length; i++)
        {
            //get data
            string line = lines[i];
            string[] obj = line.Split(',');
            if (obj.Length < headers.Length) { continue; }

            //get vectors from parsed data
            try
            {
                //convert values
                float timeMS = float.Parse(obj[time]);
                Vector3 position = new Vector3(float.Parse(obj[posX]), float.Parse(obj[posY]), float.Parse(obj[posZ]));
                Vector3 forward = new Vector3(float.Parse(obj[fwdX]), float.Parse(obj[fwdY]), float.Parse(obj[fwdZ]));
                Vector3 up = new Vector3(float.Parse(obj[upX]), float.Parse(obj[upY]), float.Parse(obj[upZ]));

                //add to arrays
                _positions[i] = position;
                _forwards[i] = forward;
                _ups[i] = up;
                _time[i] = timeMS;
            }
            catch
            {
                Debug.Log("Line Error");
            }
        }
    }
}