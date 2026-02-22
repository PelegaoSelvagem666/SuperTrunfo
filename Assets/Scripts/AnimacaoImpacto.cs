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
        // Garante que comece invisível e desativado para não atrapalhar cliques
        imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, 0f);
        imagemBrlho.raycastTarget = false; 
    }

    public void Explodir()
    {
        StartCoroutine(RotinaOndaDeChoque());
    }

    private IEnumerator RotinaOndaDeChoque()
    {
        // 1. Fica totalmente opaco e pequeno
        imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, 1f);
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        float alpha = 1f;

        // 2. Cresce violentamente enquanto fica transparente
        while (alpha > 0f)
        {
            transform.localScale += Vector3.one * velocidadeExpansao * Time.deltaTime;
            alpha -= Time.deltaTime * 3f; // Some em 1/3 de segundo
            
            imagemBrlho.color = new Color(imagemBrlho.color.r, imagemBrlho.color.g, imagemBrlho.color.b, alpha);
            yield return null; // Espera o próximo frame
        }
    }
}