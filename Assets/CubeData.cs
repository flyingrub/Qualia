using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeData : MonoBehaviour {

    public float lightRange;
    public float lightIntensity;
    public Color color;
    public Vector3 scale;

    public static Color globalColor;
    public static int currentId = 0;
    private int musicId;
    private float smoothedVolume = 0;

	// Use this for initialization
	void Start () {
        if (currentId >= Instanciate.singleton.dsps.Count) currentId = 0;
        this.musicId = currentId++;
        this.color = Color.HSVToRGB((float)(this.musicId)/12f , 1, 1);
        updateLight(0);
    }

    public void randomScale()
    {
        float x = Random.value + 1;
        float y = Random.value + 1;
        float z = Random.value + 1;
        this.scale = new Vector3(x,y,z);
        Debug.Log(x + " " + scale);
    }

    public void increase()
    {
        increaseLightIntesity();
        increaseLightRange();
        increaseScale();
    }

    public void decrease()
    {
        decreaseLightIntesity();
        decreaseLightRange();
        decreaseScale();
    }

    public void increaseScale()
    {
        scale *= 1.01f;
    }

    public void decreaseScale()
    {
        scale /= 1.01f;
    }

    public void increaseLightRange()
    {
        lightRange *= 1.01f;
    }

    public void decreaseLightRange()
    {
        lightRange /= 1.01f;
    }

    public void increaseLightIntesity()
    {
        lightIntensity *= 1.001f;
    }

    public void decreaseLightIntesity()
    {
        lightIntensity /= 1.001f;
    }

    public void hueRotate()
    {
        float amount = 0.002f;
        hueRotate(amount);
    }

    public static Color hueRotate(Color c, float amount)
    {
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);
        h += amount; s = 1; v = 1;
        h = h > 1 ? 0 : h;
        return Color.HSVToRGB(h, s, v);
    }

    public void hueRotate(float amount)
    {
        color = hueRotate(color, amount);
    }

    public float getRandomForce()
    {
        return scale.x * Random.Range(0, Instanciate.singleton.maxForce);
    }

    public float getRandomTorque()
    {
        return scale.x * Random.Range(0, Instanciate.singleton.maxTorque);
    }

    public void addRandomForce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 force = new Vector3(getRandomForce(), getRandomForce(), getRandomForce());
        rb.AddForce(force);
        addRandomTorque();
    }

    public void addRandomTorque()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 torque = new Vector3(getRandomTorque(), getRandomTorque(), getRandomTorque());
        rb.AddTorque(torque);
    }

    public Color getColor()
    {
        if (Instanciate.singleton.isGlobal)
        {
            return globalColor;
        }
        else
        {
            return color;
        }
    }

    private void updateLight(float rms)
    {
        Light light = GetComponent<Light>();
        light.color = getColor();
        light.range = lightRange + rms * 10;
        light.intensity = lightIntensity + rms * 10;
        Material m = GetComponent<Renderer>().material;
        m.SetColor("_EmissionColor", getColor() * lightIntensity / 10);
    }

    public void updateScale(float rms)
    {
        this.transform.localScale = scale + scale * rms * 0.5f;
    }

	// Update is called once per frame
	void Update () {
        float globalRms = Instanciate.singleton.getGlobalRmsLevel();
        float rms = Instanciate.singleton.getRmsLevel(musicId);
        float total = globalRms * 0.2f + rms * 1.8f;
        if (Instanciate.singleton.isGlobal)
        {
            total = globalRms * 2f;
        }
        smoothedVolume += (total - smoothedVolume) * 0.1f;
        updateScale(smoothedVolume);
        updateLight(smoothedVolume);
	}
}
