using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CartaArrastavel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("Dados")]
    public CardData dadosDaCarta;
    public bool ehDoCatalogo; 

    private ScrollRect scrollRectPai;
    private GameObject fantasmaDrag; 
    private Canvas canvasPrincipal;
    private bool arrastandoCarta = false;

    void Start()
    {
        scrollRectPai = GetComponentInParent<ScrollRect>();
        canvasPrincipal = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // --- MUDANÇA AQUI: REMOVEMOS A CHECAGEM DE DIREÇÃO (X > Y) ---
        // Agora, qualquer tentativa de arrastar a carta ativa o modo drag imediatamente.
        arrastandoCarta = true;
        CriarFantasma();
        
        // Dica: Como a carta sempre vai ser arrastada, o Scroll View nunca vai receber
        // o comando de rolar se você clicar em cima de uma carta. 
        // Para rolar a lista, o jogador terá que clicar nos espaços vazios ou na barra lateral.
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (arrastandoCarta)
        {
            if (fantasmaDrag != null)
            {
                fantasmaDrag.transform.position = eventData.position; 
            }
        }
        else
        {
            // Este código de "else" tecnicamente nunca mais será executado para o Drag,
            // mas mantemos por segurança caso adicione alguma lógica de bloqueio futuro.
            if (scrollRectPai != null) scrollRectPai.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (arrastandoCarta)
        {
            if (fantasmaDrag != null) Destroy(fantasmaDrag);
            arrastandoCarta = false;

            VerificarOndeSoltou(eventData);
        }
        else
        {
            if (scrollRectPai != null) scrollRectPai.OnEndDrag(eventData);
        }
    }

    // A rodinha do mouse continua funcionando normal para rolar a lista!
    public void OnScroll(PointerEventData eventData)
    {
        if (scrollRectPai != null) scrollRectPai.OnScroll(eventData);
    }

    // --- FUNÇÕES AUXILIARES (Mantidas Iguais) ---
    private void VerificarOndeSoltou(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool soltouNoDeck = false;
        bool soltouNoCatalogo = false;

        foreach (var result in results)
        {
            string nome = result.gameObject.name;
            if (nome.Contains("Scroll_DeckJogador") || (result.gameObject.transform.parent != null && result.gameObject.transform.parent.name == "Scroll_DeckJogador"))
            {
                soltouNoDeck = true;
            }
            if (nome.Contains("Scroll View") || nome.Contains("Area Content Grade")) 
            {
                soltouNoCatalogo = true;
            }
        }

        if (ehDoCatalogo && soltouNoDeck)
        {
            if (MenuDeckBuilder.instancia != null) MenuDeckBuilder.instancia.AdicionarCartaAoDeck(dadosDaCarta);
        }
        else if (!ehDoCatalogo && soltouNoCatalogo)
        {
            if (MenuDeckBuilder.instancia != null) MenuDeckBuilder.instancia.RemoverCartaDoDeck(dadosDaCarta);
        }
    }

    private void CriarFantasma()
    {
        fantasmaDrag = new GameObject("FantasmaCarta");
        if (canvasPrincipal != null) fantasmaDrag.transform.SetParent(canvasPrincipal.transform); 
        else fantasmaDrag.transform.SetParent(transform.root);

        Image imgOriginal = transform.Find("Arte").GetComponent<Image>(); 
        Image imgFantasma = fantasmaDrag.AddComponent<Image>();
        imgFantasma.sprite = imgOriginal.sprite;
        imgFantasma.preserveAspect = true;

        Color cor = imgFantasma.color;
        cor.a = 0.8f; 
        imgFantasma.color = cor;
        
        // Mantendo o tamanho maior que você pediu
        fantasmaDrag.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        CanvasGroup cg = fantasmaDrag.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; 
    }
}