using UnityEngine;
using UnityEngine.UI;

public class BlasterUI : MonoBehaviour
{
    [SerializeField] Blaster[] _blasters;
    [SerializeField] Image[] _blasterBars, _coolDownBars;

    void LateUpdate()
    {
        for (int i = 0; i < _blasters.Length; ++i)
        {
            _blasterBars[i].fillAmount = Mathf.Lerp(_blasterBars[i].fillAmount, 
                _blasters[i].CapacitorChargePercentage,
                10f * Time.deltaTime);
            _coolDownBars[i].fillAmount = Mathf.Lerp(_coolDownBars[i].fillAmount,
                _blasters[i].CoolDownPercent, 
                10 * Time.deltaTime);
        }
    }
}
