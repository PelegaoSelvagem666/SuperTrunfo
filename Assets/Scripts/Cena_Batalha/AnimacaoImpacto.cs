using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class AnimacaoImpacto : MonoBehaviour
{
    public float velocidadeExpansao = 8f;
    private Image imagemBrlho;

    void Start()
    {
        imagemBrlho = GetComponent<Image>();
        imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, 0f);
        imagemBrlho.raycastTarget = false; 
    }

    public void Explodir()
    {
        StartCoroutine(RotinaOndaDeChoque());
    }

    private IEnumerator RotinaOndaDeChoque()
    {
        imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, 1f);
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        float alpha = 1f;

        while (alpha > 0f)
        {
            transform.localScale += Vector3.one * velocidadeExpansao * Time.deltaTime;
            alpha -= Time.deltaTime * 3f;
            
            imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, alpha);
            yield return null;
        }
    }
}