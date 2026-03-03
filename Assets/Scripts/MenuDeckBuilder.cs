using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq; 
using UnityEngine.SceneManagement;

public class MenuDeckBuilder : MonoBehaviour
{
    public static MenuDeckBuilder instancia; 

    void Awake() 
    {
        instancia = this;
    }
    [Header("Filtros e Pesquisa")]
    public TMP_InputField campoPesquisa;
    public TMP_Dropdown dropdownTipo;
    public TMP_Dropdown dropdownOrdem;
    
    // Esta é a lista que vai aparecer na tela (pode ser todas ou só algumas)
    private List<CardData> cartasFiltradas = new List<CardData>(); 
    // ==========================================

    [Header("Configurações do Catálogo")]
    public GameObject cartaMenuPrefab; 
    public Transform areaContentGrade;     
    
    [Header("Configurações do Deck do Jogador")]
    public Transform areaContentDeck; 
    public TextMeshProUGUI textoContadorDeck; 

    [Header("Limites de Cartas por Classe")]
    public int limiteSS = 2;
    public int limiteS = 4;
    public int limiteA = 6;
    public int limiteB = 8;
    public int limiteC = 25;

    [Header("Todas as Cartas do Jogo")]
    public List<CardData> acervoCompleto; 

    [System.Serializable] public struct FrameMapping { public CardClass classe; public Sprite molduraSprite; }
    [Header("Mapeamento de Molduras")] public List<FrameMapping> listaMolduras;

    [System.Serializable] public struct ClassLetterMapping { public CardClass classe; public Sprite letraSprite; }
    [Header("Mapeamento de Classes")] public List<ClassLetterMapping> listaLetras;

    [System.Serializable] public struct TipoIconeMapping { public CardType tipo; public Sprite icone; }
    [Header("Mapeamento de Tipos")] public List<TipoIconeMapping> iconesTipos;

    [Header("Painel de Detalhes (Esquerda)")]
    public GameObject painelObjetoPrincipal;
    public Image detalheArte;
    public Image detalheMoldura;
    public Image detalheIconeClasse;
    public Image detalheIconeTipo;
    public TextMeshProUGUI detalheTxtNome;
    public TextMeshProUGUI detalheTxtForca;
    public TextMeshProUGUI detalheTxtMagia;
    public TextMeshProUGUI detalheTxtAgilidade;
    public TextMeshProUGUI detalheTxtInteligencia;
    public TextMeshProUGUI detalheTxtHabilidade; 

    private CardData cartaSendoVisualizada; 

    void Start()
    {
        CarregarCartasAutomaticamente();
        
        // NOVO: Em vez de gerar todas de uma vez, rodamos o filtro primeiro!
        AplicarFiltros(); 
        
        CarregarDeckSalvo(); 
    }

    private void CarregarCartasAutomaticamente()
    {
        CardData[] todasAsCartasDaPasta = Resources.LoadAll<CardData>("Cartas");
        acervoCompleto = new List<CardData>(todasAsCartasDaPasta);
    }

public void AplicarFiltros()
    {
        cartasFiltradas = new List<CardData>(acervoCompleto);

        // 2. Filtro de Nome (Agora ele limpa espaços em branco e códigos invisíveis do TMP!)
        if (campoPesquisa != null && !string.IsNullOrEmpty(campoPesquisa.text))
        {
            string termo = campoPesquisa.text.ToLower().Replace("\u200B", "").Trim();
            
            if (!string.IsNullOrEmpty(termo))
            {
                cartasFiltradas = cartasFiltradas.FindAll(c => c.nomeCarta.ToLower().Contains(termo));
            }
        }

        // 3. Filtro de Tipo (Com '.Trim()' para ignorar espaços acidentais nas opções)
        if (dropdownTipo != null && dropdownTipo.value > 0) 
        {
            string tipoEscolhido = dropdownTipo.options[dropdownTipo.value].text.Trim();
            cartasFiltradas = cartasFiltradas.FindAll(c => c.tipo.ToString() == tipoEscolhido);
        }

        // 4. Ordenação
        if (dropdownOrdem != null)
        {
            if (dropdownOrdem.value == 1) cartasFiltradas = cartasFiltradas.OrderBy(c => c.classe).ToList();
            else if (dropdownOrdem.value == 2) cartasFiltradas = cartasFiltradas.OrderByDescending(c => c.classe).ToList();
        }

        GerarCatalogoNaTela();
    }
    private void GerarCatalogoNaTela()
    {
        // Limpa a tela
        foreach (Transform filho in areaContentGrade) Destroy(filho.gameObject);

        // NOVO: Usa a lista "cartasFiltradas" em vez de "acervoCompleto"
        foreach (CardData carta in cartasFiltradas) 
        {
            GameObject novaCarta = CriarVisualDaCarta(carta, areaContentGrade);
            ConfigurarCliquesDaCarta(novaCarta, carta, true); 
            CartaArrastavel dragScript = novaCarta.GetComponent<CartaArrastavel>();
            if (dragScript != null)
            {
                dragScript.dadosDaCarta = carta;
                dragScript.ehDoCatalogo = true; // Só permitimos arrastar se vier do catálogo
            }
        }
    }

    private void AtualizarVisualDoDeck()
    {
        foreach (Transform filho in areaContentDeck) Destroy(filho.gameObject);

        foreach (CardData carta in GerenciadorDeDeck.instancia.deckAtivo)
        {
            GameObject novaCarta = CriarVisualDaCarta(carta, areaContentDeck);
            ConfigurarCliquesDaCarta(novaCarta, carta, false); 
            CartaArrastavel dragScript = novaCarta.GetComponent<CartaArrastavel>();
            if (dragScript != null)
            {
                dragScript.dadosDaCarta = carta;
                dragScript.ehDoCatalogo = false; 
            }
        }

        if (textoContadorDeck != null)
        {
            int total = GerenciadorDeDeck.instancia.deckAtivo.Count;
            textoContadorDeck.text = $"Cartas: {total} / 25";
            if (total == 25) textoContadorDeck.color = Color.green;
            else textoContadorDeck.color = Color.white;
        }
    }

    private int ContarCartasDaClasse(CardClass classeAlvo)
    {
        int contagem = 0;
        foreach (CardData carta in GerenciadorDeDeck.instancia.deckAtivo)
        {
            if (carta.classe == classeAlvo) contagem++;
        }
        return contagem;
    }

 private void ConfigurarCliquesDaCarta(GameObject objCarta, CardData carta, bool ehDoCatalogo)
    {
        EventTrigger trigger = objCarta.GetComponent<EventTrigger>();
        if (trigger == null) trigger = objCarta.AddComponent<EventTrigger>();
        trigger.triggers.Clear(); 

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) =>
        {
            PointerEventData pointerData = (PointerEventData)data;
            
            if (pointerData.button == PointerEventData.InputButton.Left) 
            {
                MostrarDetalhesDaCarta(carta);
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                if (ehDoCatalogo) AdicionarCartaAoDeck(carta); 
                else RemoverCartaDoDeck(carta);   
            }

            // --- A MÁGICA ANTI-BUG DO SCROLL ---
            // Isso tira o "foco" da carta assim que você clica nela, 
            // liberando a rodinha do mouse para continuar rolando o catálogo!
            EventSystem.current.SetSelectedGameObject(null);
        });
        
        trigger.triggers.Add(entry);
    }

    private GameObject CriarVisualDaCarta(CardData carta, Transform pai)
    {
        GameObject novaCarta = Instantiate(cartaMenuPrefab, pai);
        Transform arteObj = novaCarta.transform.Find("Arte");
        Transform molduraObj = novaCarta.transform.Find("Moldura");
        Transform iconeClasseObj = novaCarta.transform.Find("IconeClasse");
        Transform iconeTipoObj = novaCarta.transform.Find("IconeTipo");
        Transform txtNomeObj = novaCarta.transform.Find("TextoNome");
        Transform txtForcaObj = novaCarta.transform.Find("TextoForca");
        Transform txtMagiaObj = novaCarta.transform.Find("TextoMagia");
        Transform txtAgilidadeObj = novaCarta.transform.Find("TextoAgilidade"); 
        Transform txtInteligenciaObj = novaCarta.transform.Find("TextoInteligencia");

        if (arteObj != null) arteObj.GetComponent<Image>().sprite = carta.arteCarta;
        if (molduraObj != null) molduraObj.GetComponent<Image>().sprite = GetMolduraPorClasse(carta.classe);
        if (iconeClasseObj != null) iconeClasseObj.GetComponent<Image>().sprite = GetLetraPorClasse(carta.classe);
        if (iconeTipoObj != null) iconeTipoObj.GetComponent<Image>().sprite = GetIconeTipo(carta.tipo);
        if (txtNomeObj != null) txtNomeObj.GetComponent<TextMeshProUGUI>().text = carta.nomeCarta;
        if (txtForcaObj != null) txtForcaObj.GetComponent<TextMeshProUGUI>().text = carta.forca.ToString();
        if (txtMagiaObj != null) txtMagiaObj.GetComponent<TextMeshProUGUI>().text = carta.magia.ToString(); 
        if (txtAgilidadeObj != null) txtAgilidadeObj.GetComponent<TextMeshProUGUI>().text = carta.agilidade.ToString(); 
        if (txtInteligenciaObj != null) txtInteligenciaObj.GetComponent<TextMeshProUGUI>().text = carta.inteligencia.ToString(); 

        return novaCarta;
    }

    private void MostrarDetalhesDaCarta(CardData carta)
    {
        cartaSendoVisualizada = carta;
        if (painelObjetoPrincipal != null) painelObjetoPrincipal.SetActive(true);
        if (detalheArte != null) detalheArte.sprite = carta.arteCarta;
        if (detalheMoldura != null) detalheMoldura.sprite = GetMolduraPorClasse(carta.classe);
        if (detalheIconeClasse != null) detalheIconeClasse.sprite = GetLetraPorClasse(carta.classe);
        if (detalheIconeTipo != null) detalheIconeTipo.sprite = GetIconeTipo(carta.tipo);
        if (detalheTxtNome != null) detalheTxtNome.text = carta.nomeCarta;
        if (detalheTxtForca != null) detalheTxtForca.text = carta.forca.ToString();
        if (detalheTxtMagia != null) detalheTxtMagia.text = carta.magia.ToString();
        if (detalheTxtAgilidade != null) detalheTxtAgilidade.text = carta.agilidade.ToString();
        if (detalheTxtInteligencia != null) detalheTxtInteligencia.text = carta.inteligencia.ToString();
        if (detalheTxtHabilidade != null) detalheTxtHabilidade.text = carta.descricaoHabilidade;
    }

    public void AdicionarCartaAoDeck(CardData carta)
    {
        if (GerenciadorDeDeck.instancia.deckAtivo.Contains(carta)) { Debug.LogWarning($"A carta {carta.nomeCarta} já está no deck!"); return; }
        if (GerenciadorDeDeck.instancia.deckAtivo.Count >= 25) { Debug.LogWarning("O deck já está cheio!"); return; }

        int qtdAtualNaClasse = ContarCartasDaClasse(carta.classe);
        int limitePermitido = 25; 
        switch (carta.classe)
        {
            case CardClass.SS: limitePermitido = limiteSS; break;
            case CardClass.S:  limitePermitido = limiteS;  break;
            case CardClass.A:  limitePermitido = limiteA;  break;
            case CardClass.B:  limitePermitido = limiteB;  break;
            case CardClass.C:  limitePermitido = limiteC;  break;
        }

        if (qtdAtualNaClasse >= limitePermitido) { Debug.LogWarning($"Limite atingido! Máximo de {limitePermitido} cartas da classe {carta.classe}."); return; }

        GerenciadorDeDeck.instancia.deckAtivo.Add(carta);
        AtualizarVisualDoDeck(); 
    }

    public void BotaoAdicionarCartaVisualizada() { if (cartaSendoVisualizada != null) AdicionarCartaAoDeck(cartaSendoVisualizada); }
    public void RemoverCartaDoDeck(CardData carta) { GerenciadorDeDeck.instancia.deckAtivo.Remove(carta); AtualizarVisualDoDeck(); }
    
    public void SalvarDeck()
    {
        List<string> nomesDasCartasSalvas = new List<string>();
        foreach (CardData carta in GerenciadorDeDeck.instancia.deckAtivo) nomesDasCartasSalvas.Add(carta.name); 
        string deckTexto = string.Join(",", nomesDasCartasSalvas);
        PlayerPrefs.SetString("MeuDeckSalvo", deckTexto);
        PlayerPrefs.Save();
        if (GerenciadorDeDeck.instancia.deckAtivo.Count == 25) Debug.Log("✅ Deck VÁLIDO de 25 cartas salvo com sucesso!");
        else Debug.Log($"⚠️ Deck salvo como RASCUNHO ({GerenciadorDeDeck.instancia.deckAtivo.Count}/25).");
    }

    private void CarregarDeckSalvo()
    {
        if (PlayerPrefs.HasKey("MeuDeckSalvo"))
        {
            GerenciadorDeDeck.instancia.deckAtivo.Clear(); 
            string deckTexto = PlayerPrefs.GetString("MeuDeckSalvo");
            if (!string.IsNullOrEmpty(deckTexto))
            {
                string[] nomes = deckTexto.Split(','); 
                foreach (string nomeArquivo in nomes)
                {
                    CardData cartaEncontrada = acervoCompleto.Find(c => c.name == nomeArquivo);
                    if (cartaEncontrada != null) GerenciadorDeDeck.instancia.deckAtivo.Add(cartaEncontrada);
                }
            }
        }
        AtualizarVisualDoDeck(); 
    }
    public void VoltarAoMenuPrincipal()
    {
        // Garante que o jogador salvou antes de sair (opcional, mas recomendado)
        SalvarDeck(); 
        SceneManager.LoadScene("MenuPrincipal");
    }

    private Sprite GetMolduraPorClasse(CardClass classeProcurada) { foreach (var item in listaMolduras) { if (item.classe == classeProcurada) return item.molduraSprite; } return null; }
    private Sprite GetLetraPorClasse(CardClass classeProcurada) { foreach (var item in listaLetras) { if (item.classe == classeProcurada) return item.letraSprite; } return null; }
    private Sprite GetIconeTipo(CardType tipoProcurado) { foreach (var mapping in iconesTipos) { if (mapping.tipo == tipoProcurado) return mapping.icone; } return null; }
}