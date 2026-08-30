using UnityEngine;
using TMPro;

public class HDRTextGlow : MonoBehaviour
{
	private TextMeshProUGUI tmpText;
	void Start()
	{
		tmpText = GetComponent<TextMeshProUGUI>();
		Material textMaterial = tmpText.fontSharedMaterial;
		float intensity = 1.55f;
		Color hdrColor = new Color(0.580f, 0.404f, 0.271f) * intensity;
		textMaterial.SetColor("_FaceColor", hdrColor);
	}
}
