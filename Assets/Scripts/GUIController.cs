
using UnityEngine;
using UnityEngine.UI;

//Controls GUI
public class GUIController : MonoBehaviour
{
    public Slider manaSlider;
    public Slider healthSlider;
    void UpdateMana(float mana)
    {
        manaSlider.value = mana;
    }

    void UpdateHealth (float health)
    {
        healthSlider.value = health;
    }
}
