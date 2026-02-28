using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Dados")]
    public CardData cardData;
    public bool pertenceAoJogador = false;

    [Header("Estrutura Visual")]
    public Image imagemVerso;
    public Image camadaArte;
    public Image camadaMoldura;
    public Image iconeClasseLetra;

    [Header("Textos e Ícones")]
    public TextMeshProUGUI textoNome;
    public Image imagemTipo1;
    public Image imagemTipo2; 
    public Image imagemMoral;
    public TextMeshProUGUI textoForca;
    public TextMeshProUGUI textoMagia;
    public TextMeshProUGUI textoAgilidade;
    public TextMeshProUGUI textoInteligencia;
    public TextMeshProUGUI textoHabTitulo;
    public TextMeshProUGUI textoHabDesc;

    [System.Serializable]
    public struct FrameMapping { public CardClass classe; public Sprite molduraSprite; }
    public List<FrameMapping> listaMolduras;

    [System.Serializable]
    public struct ClassLetterMapping { public CardClass classe; public Sprite letraSprite; }
    public List<ClassLetterMapping> listaLetras;

    // VOLTAMOS COM A LISTA AQUI!
    [System.Serializable]
    public struct TipoIconeMapping { public CardType tipo; public Sprite icone; }
    public List<TipoIconeMapping> iconesTipos;

    [Header("Ícones Morais")]
    public Sprite iconeBom;
    public Sprite iconeNeutro;
    public Sprite iconeMal;

    private Transform parentOriginal;
    private Vector3 posicaoOriginal;
    private int indiceOriginal;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (imagemVerso != null) imagemVerso.gameObject.SetActive(!pertenceAoJogador);
        if (cardData != null) AtualizarCarta();
    }

    public void AtualizarCarta()
    {
        if (cardData == null) return;
        if (imagemVerso != null && imagemVerso.gameObject.activeSelf) return;

        if (camadaArte != null) camadaArte.sprite = cardData.arteCarta;
        if (textoNome != null) textoNome.text = cardData.nomeCarta;
        if (camadaMoldura != null) camadaMoldura.sprite = GetMolduraPorClasse(cardData.classe);
        if (iconeClasseLetra != null) iconeClasseLetra.sprite = GetLetraPorClasse(cardData.classe);

        if (textoForca != null) textoForca.text = cardData.forca.ToString();
        if (textoMagia != null) textoMagia.text = cardData.magia.ToString();
        if (textoAgilidade != null) textoAgilidade.text = cardData.agilidade.ToString();
        if (textoInteligencia != null) textoInteligencia.text = cardData.inteligencia.ToString();

        if (textoHabTitulo != null) textoHabTitulo.text = cardData.tituloHabilidade;
        if (textoHabDesc != null) textoHabDesc.text = cardData.descricaoHabilidade;

        if (imagemMoral != null)
        {
            switch (cardData.moral)
            {
                case CardMoral.Bom: imagemMoral.sprite = iconeBom; break;
                case CardMoral.Neutro: imagemMoral.sprite = iconeNeutro; break;
                case CardMoral.Mau: imagemMoral.sprite = iconeMal; break;
            }
        }

        // Puxa o ícone baseado no Enum da carta
        if (imagemTipo1 != null) imagemTipo1.sprite = GetIconeTipo(cardData.tipo); 
    }

    private Sprite GetMolduraPorClasse(CardClass classeProcurada)
    {
        foreach (var item in listaMolduras) { if (item.classe == classeProcurada) return item.molduraSprite; }
        return null;
    }

    private Sprite GetLetraPorClasse(CardClass classeProcurada)
    {
        foreach (var item in listaLetras) { if (item.classe == classeProcurada) return item.letraSprite; }
        return null;
    }

    // Método restaurado
    private Sprite GetIconeTipo(CardType tipoProcurado)
    {
        foreach (var mapping in iconesTipos) { if (mapping.tipo == tipoProcurado) return mapping.icone; }
        return null;
    }

    // --- INTERAÇÕES DE MOUSE E CLIQUE (MANTIDAS IGUAIS) ---
    public void OnPointerDown(PointerEventData eventData) { /* ... código original ... */ if (pertenceAoJogador || (!pertenceAoJogador && imagemVerso != null && !imagemVerso.gameObject.activeSelf)) GameManager.instancia.InspecionarCarta(cardData); }
    public void OnPointerEnter(PointerEventData eventData) { /* ... código original ... */ if (pertenceAoJogador && !GameManager.instancia.jogoPausado && transform.parent == GameManager.instancia.maoJogador) { transform.localScale = new Vector3(1.15f, 1.15f, 1.15f); transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 20f, transform.localPosition.z); } }
    public void OnPointerExit(PointerEventData eventData) { /* ... código original ... */ if (pertenceAoJogador && !GameManager.instancia.jogoPausado && transform.parent == GameManager.instancia.maoJogador) { transform.localScale = Vector3.one; transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 20f, transform.localPosition.z); } }
    public void OnBeginDrag(PointerEventData eventData) { /* ... código original ... */ if (!pertenceAoJogador || GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return; parentOriginal = transform.parent; indiceOriginal = transform.GetSiblingIndex(); posicaoOriginal = transform.localPosition; transform.SetParent(transform.root); canvasGroup.blocksRaycasts = false; transform.localScale = new Vector3(0.65f, 0.65f, 0.65f); }
    public void OnDrag(PointerEventData eventData) { /* ... código original ... */ if (!pertenceAoJogador || GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return; transform.position = eventData.position; }
    public void OnEndDrag(PointerEventData eventData) { /* ... código original ... */ if (!pertenceAoJogador || GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return; canvasGroup.blocksRaycasts = true; if (eventData.position.y > Screen.height * 0.4f) { transform.position = new Vector3((Screen.width / 2) - 250, Screen.height / 2, 0); transform.localScale = new Vector3(0.65f, 0.65f, 0.65f); GameManager.instancia.ReceberCartaNaArena(this); } else { transform.SetParent(parentOriginal); transform.SetSiblingIndex(indiceOriginal); transform.localPosition = posicaoOriginal; transform.localScale = Vector3.one; } }
}