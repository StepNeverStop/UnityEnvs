using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class InputHandler
{
    [Range(-1f, 1f)]
    public float bowThruster;

    [Range(-1f, 1f)]
    public float sternThruster;

    [Range(-1f, 1f)]
    public float rudder;

    [Range(-1f, 1f)]
    public float leftThrottle;

    [Range(-1f, 1f)]
    public float rightThrottle;

    [Range(-1f, 1f)]
    public float throttle;

    public Slider throttleSlider;
    public Slider leftThrottleSlider;
    public Slider rightThrottleSlider;
    public Slider rudderSlider;
    public Slider bowThrusterSlider;
    public Slider sternThrusterSlider;

    private float prevVertical;
    private float prevHorizontal;
    private float prevBowThruster;
    private float prevSternThruster;

    public void Initialize(AdvancedShipController sc)
    {
        if (throttleSlider != null) throttleSlider.onValueChanged.AddListener(delegate { SetThrottle(); });
        if (leftThrottleSlider != null) leftThrottleSlider.onValueChanged.AddListener(delegate { SetLeftThrottle(); });
        if (rightThrottleSlider != null) rightThrottleSlider.onValueChanged.AddListener(delegate { SetRightThrottle(); });
        if (rudderSlider != null) rudderSlider.onValueChanged.AddListener(delegate { SetRudder(); });
        if (bowThrusterSlider != null) bowThrusterSlider.onValueChanged.AddListener(delegate { SetBowThruster(); });
        if (sternThrusterSlider != null) sternThrusterSlider.onValueChanged.AddListener(delegate { SetSternThruster(); });
    }

    public void Update()
    {
        // Change input values if keyboard input has changed, else ignore the values.
        if (prevVertical != Input.GetAxis("Vertical"))
        {
            throttle = Input.GetAxis("Vertical");
            if (leftThrottleSlider != null && rightThrottleSlider != null)
            {
                leftThrottleSlider.value = rightThrottleSlider.value = throttle;
            }
            if(throttleSlider != null)
            {
                throttleSlider.value = throttle;
            }
        }
        if (prevHorizontal != Input.GetAxis("Horizontal"))
        {
            rudder = Input.GetAxis("Horizontal");
            if(rudderSlider != null) rudderSlider.value = rudder;
        }
        //if (prevBowThruster != Input.GetAxis("BowThruster"))
        //{
        //    bowThruster = Input.GetAxis("BowThruster");
        //    if(bowThrusterSlider != null) bowThrusterSlider.value = bowThruster;
        //}
        //if (prevSternThruster != Input.GetAxis("SternThruster"))
        //{
        //    sternThruster = Input.GetAxis("SternThruster");
        //    if(sternThrusterSlider != null) sternThrusterSlider.value = sternThruster;
        //}

        // Remember last axis values
        prevVertical = Input.GetAxis("Vertical"); ;
        prevHorizontal = Input.GetAxis("Horizontal");
        //prevBowThruster = Input.GetAxis("BowThruster");
        //prevSternThruster = Input.GetAxis("SternThruster");

        // Clamp input values to -1, 1
        throttle = Mathf.Clamp(throttle, -1f, 1f);
        leftThrottle = Mathf.Clamp(leftThrottle, -1f, 1f);
        rightThrottle = Mathf.Clamp(rightThrottle, -1f, 1f);
        bowThruster = Mathf.Clamp(bowThruster, -1f, 1f);
        sternThruster = Mathf.Clamp(sternThruster, -1f, 1f);
        rudder = Mathf.Clamp(rudder, -1f, 1f);
    }

    public void SetLeftThrottle()
    {
        leftThrottle = leftThrottleSlider.value;
    }

    public void SetRightThrottle()
    {
        rightThrottle = rightThrottleSlider.value;
    }

    public void SetRudder()
    {
        rudder = rudderSlider.value;
    }

    public void SetBowThruster()
    {
        bowThruster = bowThrusterSlider.value;
    }

    public void SetSternThruster()
    {
        sternThruster = sternThrusterSlider.value;
    }

    public void SetThrottle()
    {
        throttle = throttleSlider.value;
        leftThrottleSlider.value = throttle;
        rightThrottleSlider.value = throttle;
    }

}

