using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// Substitua a linha da classe por esta:
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

    [System.Serializable]
    public struct TipoIconeMapping { public CardType tipo; public Sprite icone; }
    public List<TipoIconeMapping> iconesTipos;

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

        // CORREÇÃO: A carta decide se terá verso apenas uma vez, ao ser gerada na mão
        if (imagemVerso != null)
        {
            imagemVerso.gameObject.SetActive(!pertenceAoJogador);
        }

        if (cardData != null) AtualizarCarta();
    }

    public void AtualizarCarta()
    {
        if (cardData == null) return;

        // Se o verso estiver ligado, o código para aqui e economiza processamento.
        // Se o GameManager desligou o verso na arena, o código prossegue e renderiza a frente.
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
                case CardMoral.Mal: imagemMoral.sprite = iconeMal; break;
            }
        }

        if (imagemTipo1 != null) imagemTipo1.sprite = GetIconeTipo(cardData.tipo1);
        if (imagemTipo2 != null) imagemTipo2.sprite = GetIconeTipo(cardData.tipo2);
    }

    Sprite GetMolduraPorClasse(CardClass classeProcurada)
    {
        foreach (var item in listaMolduras) { if (item.classe == classeProcurada) return item.molduraSprite; }
        return null;
    }

    Sprite GetLetraPorClasse(CardClass classeProcurada)
    {
        foreach (var item in listaLetras) { if (item.classe == classeProcurada) return item.letraSprite; }
        return null;
    }

    Sprite GetIconeTipo(CardType tipoProcurado)
    {
        foreach (var mapping in iconesTipos) { if (mapping.tipo == tipoProcurado) return mapping.icone; }
        return null;
    }

public void FuiClicada()
    {
        // Regra 1: Se a carta é minha, sempre posso inspecionar.
        bool ehMinhaCarta = pertenceAoJogador;

        // Regra 2: Se é do oponente, só posso inspecionar se ela já foi revelada na arena.
        // Sabemos que ela foi revelada se o objeto do verso (a lava/portal) estiver desligado.
        bool oponenteRevelado = !pertenceAoJogador && imagemVerso != null && !imagemVerso.gameObject.activeSelf;

        // Se qualquer uma das duas regras for verdadeira, chama o painel gigante.
        if (ehMinhaCarta || oponenteRevelado)
        {
            GameManager.instancia.InspecionarCarta(cardData);
        }
    }
public void OnPointerDown(PointerEventData eventData)
    {
        FuiClicada();
    }
    // SUBSTITUA a função OnPointerClick por esta nova:
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Levanta a carta SOMENTE se ela estiver fisicamente dentro da área da mão do jogador
        if (pertenceAoJogador && !GameManager.instancia.jogoPausado && transform.parent == GameManager.instancia.maoJogador)
        {
            transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 20f, transform.localPosition.z);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Retorna ao normal com a mesma restrição
        if (pertenceAoJogador && !GameManager.instancia.jogoPausado && transform.parent == GameManager.instancia.maoJogador)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 20f, transform.localPosition.z);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!pertenceAoJogador) return;
        if (GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return;

        parentOriginal = transform.parent;
        indiceOriginal = transform.GetSiblingIndex();
        posicaoOriginal = transform.localPosition;

        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;

        // NOVO: Encolhe a carta IMEDIATAMENTE quando você começa a arrastar!
        transform.localScale = new Vector3(0.65f, 0.65f, 0.65f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!pertenceAoJogador) return;
        if (GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!pertenceAoJogador) return;
        if (GameManager.instancia.cartaDoJogadorNaArena != null || GameManager.instancia.jogoPausado) return;

        canvasGroup.blocksRaycasts = true;

        if (eventData.position.y > Screen.height * 0.4f)
        {
            transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            transform.localScale = new Vector3(0.65f, 0.65f, 0.65f); 
            
            // NOVO: Chama a arena direto, em vez de simular um clique
            GameManager.instancia.ReceberCartaNaArena(this);
        }
        else
        {
            transform.SetParent(parentOriginal);
            transform.SetSiblingIndex(indiceOriginal);
            transform.localPosition = posicaoOriginal;
            transform.localScale = Vector3.one; 
        }
    }
 }
