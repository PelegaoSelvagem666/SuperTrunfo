using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimacaoBotaoAtributo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual do Efeito")]
    public Image imagemBrilho; 
    public Color corNormal = new Color(1f, 0.8f, 0f, 0.4f); 
    public Color corHover = new Color(1f, 0.9f, 0f, 0.9f);  

    [Header("Configurações de Pulso")]
    public float velocidadePulso = 4f;
    public float intensidadePulso = 0.2f;

    private bool mouseEmCima = false;
    private Vector3 escalaOriginal;

    // A MÁGICA FOI AQUI: Trocamos Start por Awake!
    // O Awake roda no frame zero, blindando contra o GameManager desligar a tela.
    void Awake() 
    {
        if (imagemBrilho != null)
        {
            escalaOriginal = imagemBrilho.transform.localScale;
        }
    }

    void OnEnable()
    {
        mouseEmCima = false;
        if (imagemBrilho != null)
        {
            // Agora ele tem a memória do tamanho real e não esmaga mais a imagem!
            imagemBrilho.transform.localScale = escalaOriginal;
            imagemBrilho.color = corNormal;
        }
    }

    void Update()
    {
        if (imagemBrilho == null) return;

        if (!mouseEmCima)
        {
            float alphaAnimado = corNormal.a + Mathf.Sin(Time.time * velocidadePulso) * intensidadePulso;
            imagemBrilho.color = new Color(corNormal.r, corNormal.g, corNormal.b, Mathf.Clamp01(alphaAnimado));
            imagemBrilho.transform.localScale = Vector3.Lerp(imagemBrilho.transform.localScale, escalaOriginal, Time.deltaTime * 10f);
        }
        else
        {
            imagemBrilho.color = corHover;
            imagemBrilho.transform.localScale = Vector3.Lerp(imagemBrilho.transform.localScale, escalaOriginal * 1.15f, Time.deltaTime * 15f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { mouseEmCima = true; }
    public void OnPointerExit(PointerEventData eventData) { mouseEmCima = false; }
}