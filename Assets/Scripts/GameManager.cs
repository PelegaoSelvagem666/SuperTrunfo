using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Configurações do Baralho")]
    public BotProfile oponenteAtual; 
    public List<CardData> baralhoJogador = new List<CardData>(); 
    public List<CardData> baralhoOponente = new List<CardData>(); 
    public GameObject cartaPrefab;
    private int indiceCompra = 0;

    [Header("Áreas do Tabuleiro")]
    public Canvas canvasPrincipal;
    public Transform maoJogador;
    public Transform maoAdversario;
    public Transform cemiterioJogador;
    public Transform cemiterioOponente;

    [Header("Interface de Batalha")]
    public GameObject painelEscolhaAtributo; 
    public CardDisplay painelCartaDetalhe; 
    public TextMeshProUGUI textoResultado; 
    public TextMeshProUGUI textoAvisoIA;
    public AnimacaoImpacto efeitoChoque;
    public GameObject fundoInspecao;
    
    [Header("Imagens de Fim de Rodada/Jogo")]
    public Image imgVitoriaRodada;
    public Image imgDerrotaRodada;
    public Image imgEmpateRodada; 
    public Image imgVitoriaJogo;
    public Image imgDerrotaJogo;
    public Image imgEmpateJogo;   
    
    [Header("Placares Visuais")]
    public TextMeshProUGUI txtPontosJogador;  
    public TextMeshProUGUI txtPontosOponente; 
    public TextMeshProUGUI txtVitoriasJogador;
    public TextMeshProUGUI txtVitoriasOponente;
    
    [Header("Estado do Jogo")]
    public bool turnoDoJogador = true; 
    public CardDisplay cartaDoJogadorNaArena; 
    public bool jogoPausado = false; 

    [Header("Visualizador de Cemitério")]
    public GameObject painelVisualizadorCemiterio;
    public Transform conteudoGradeCemiterio; 
    public GameObject prefabIconeCemiterio;  

    private int pontosJogador = 0;
    private int pontosOponente = 0;
    private int vitoriasJogador = 0;
    private int vitoriasOponente = 0;

    private CardDisplay cartaAtacanteIA; 
    private string atributoAtaqueIA;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        if (painelEscolhaAtributo != null) painelEscolhaAtributo.SetActive(false);
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);

        DesativarImagensDeResultado(); 

        vitoriasJogador = 0;
        vitoriasOponente = 0;
        pontosJogador = 0;
        pontosOponente = 0;
        indiceCompra = 0;
        AtualizarPlacares();

        CarregarDeckSalvoDoJogador();

        if (oponenteAtual != null)
        {
            baralhoOponente = new List<CardData>(oponenteAtual.deckPreDefinido);
        }
        else
        {
            Debug.LogError("BotProfile não atribuído no Inspector!");
        }

        EmbaralharDeck(baralhoJogador);
        EmbaralharDeck(baralhoOponente);
        StartCoroutine(DistribuirCartasAnimado());
    } 

    private void CarregarDeckSalvoDoJogador()
    {
        baralhoJogador.Clear();
        CardData[] todasAsCartas = Resources.LoadAll<CardData>("Cartas");
        List<CardData> acervoCompleto = new List<CardData>(todasAsCartas);

        if (PlayerPrefs.HasKey("MeuDeckSalvo"))
        {
            string deckTexto = PlayerPrefs.GetString("MeuDeckSalvo");
            if (!string.IsNullOrEmpty(deckTexto))
            {
                string[] nomes = deckTexto.Split(',');
                foreach (string nomeArquivo in nomes)
                {
                    CardData cartaEncontrada = acervoCompleto.Find(c => c.name == nomeArquivo);
                    if (cartaEncontrada != null) baralhoJogador.Add(cartaEncontrada);
                }
            }
        }
        
        if(baralhoJogador.Count == 0) Debug.LogError("Deck do jogador não encontrado ou vazio.");
    }

    private void DesativarImagensDeResultado()
    {
        if (imgVitoriaRodada != null) imgVitoriaRodada.gameObject.SetActive(false);
        if (imgDerrotaRodada != null) imgDerrotaRodada.gameObject.SetActive(false);
        if (imgEmpateRodada != null) imgEmpateRodada.gameObject.SetActive(false);
        if (imgVitoriaJogo != null) imgVitoriaJogo.gameObject.SetActive(false);
        if (imgDerrotaJogo != null) imgDerrotaJogo.gameObject.SetActive(false);
        if (imgEmpateJogo != null) imgEmpateJogo.gameObject.SetActive(false);
        if (textoResultado != null) textoResultado.gameObject.SetActive(false);
    }

    public void EmbaralharDeck(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    private IEnumerator DistribuirCartasAnimado()
    {
        for (int i = 0; i < 5; i++)
        {
            if (indiceCompra < baralhoJogador.Count)
            {
                GameObject novaCartaJogador = Instantiate(cartaPrefab, maoJogador);
                CardDisplay displayJogador = novaCartaJogador.GetComponent<CardDisplay>();
                displayJogador.cardData = baralhoJogador[indiceCompra];
                displayJogador.pertenceAoJogador = true;
                displayJogador.AtualizarCarta();
            }

            if (indiceCompra < baralhoOponente.Count)
            {
                GameObject novaCartaAdversario = Instantiate(cartaPrefab, maoAdversario);
                CardDisplay displayAdversario = novaCartaAdversario.GetComponent<CardDisplay>();
                displayAdversario.cardData = baralhoOponente[indiceCompra]; 
                displayAdversario.pertenceAoJogador = false;
                displayAdversario.AtualizarCarta();
            }

            indiceCompra++; 
            yield return new WaitForSeconds(0.25f);
        }

        jogoPausado = false;

        if (!turnoDoJogador) TurnoDaIA();
        else Debug.Log("Rodada iniciada. Turno do jogador.");
    }

   private void AtualizarPlacares()
    {
        if (txtPontosJogador != null) txtPontosJogador.text = pontosJogador.ToString();
        if (txtPontosOponente != null) txtPontosOponente.text = pontosOponente.ToString();
        
        if (txtVitoriasJogador != null) txtVitoriasJogador.text = vitoriasJogador.ToString();
        if (txtVitoriasOponente != null) txtVitoriasOponente.text = vitoriasOponente.ToString();
    }

    public void InspecionarCarta(CardData dados)
    {
        if (painelCartaDetalhe == null) return;
        if (fundoInspecao != null) { fundoInspecao.SetActive(true); fundoInspecao.transform.SetAsLastSibling(); }
        if (maoJogador != null) maoJogador.SetAsLastSibling();
        if (cartaDoJogadorNaArena != null) cartaDoJogadorNaArena.transform.SetAsLastSibling();
        if (cartaAtacanteIA != null) cartaAtacanteIA.transform.SetAsLastSibling();

        if (painelVisualizadorCemiterio != null && painelVisualizadorCemiterio.activeSelf) painelVisualizadorCemiterio.transform.SetAsLastSibling();
        else
        {
            if (cemiterioJogador != null) cemiterioJogador.SetAsLastSibling(); 
            if (cemiterioOponente != null) cemiterioOponente.SetAsLastSibling(); 
        }

        painelCartaDetalhe.gameObject.SetActive(true);
        painelCartaDetalhe.transform.SetAsLastSibling(); 
        painelCartaDetalhe.cardData = dados;
        painelCartaDetalhe.AtualizarCarta();
    }

    public void ReceberCartaNaArena(CardDisplay cartaDoJogador) 
    {
        if (jogoPausado) return;
        cartaDoJogadorNaArena = cartaDoJogador; 

        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);
        if (fundoInspecao != null) fundoInspecao.SetActive(false);

        if (turnoDoJogador)
        {
            painelEscolhaAtributo.transform.SetAsLastSibling(); 
            painelEscolhaAtributo.SetActive(true);
        }
        else
        {
            ResolverDuelo(cartaDoJogadorNaArena, cartaAtacanteIA, atributoAtaqueIA);
        }
    }

    public void EscolherForca() { IniciarBatalha("Força"); }
    public void EscolherMagia() { IniciarBatalha("Magia"); }
    public void EscolherAgilidade() { IniciarBatalha("Agilidade"); }
    public void EscolherInteligencia() { IniciarBatalha("Inteligência"); }

    public void CancelarJogada()
    {
        if (cartaDoJogadorNaArena != null)
        {
            cartaDoJogadorNaArena.transform.SetParent(maoJogador, false);
            cartaDoJogadorNaArena = null; 
        }
        if (painelEscolhaAtributo != null) painelEscolhaAtributo.SetActive(false);
    }

    private void IniciarBatalha(string atributoEscolhido)
    {
        painelEscolhaAtributo.SetActive(false);
        CardDisplay cartaOponenteEscolhida = EscolherCartaDaIA(atributoEscolhido);

        if (cartaOponenteEscolhida != null)
        {
            cartaOponenteEscolhida.transform.SetParent(canvasPrincipal.transform);
            cartaOponenteEscolhida.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
            cartaOponenteEscolhida.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

            if (cartaOponenteEscolhida.imagemVerso != null)
            {
                cartaOponenteEscolhida.imagemVerso.gameObject.SetActive(false);
                cartaOponenteEscolhida.AtualizarCarta(); 
            }

            ResolverDuelo(cartaDoJogadorNaArena, cartaOponenteEscolhida, atributoEscolhido);
        }
    }

    private CardDisplay EscolherCartaDaIA(string atributo)
    {
        CardDisplay melhorCarta = null;
        int maiorValor = -1;

        foreach (Transform filho in maoAdversario)
        {
            CardDisplay display = filho.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null)
            {
                int valorAtual = PegarValorAtributo(display.cardData, atributo);
                if (valorAtual > maiorValor)
                {
                    maiorValor = valorAtual;
                    melhorCarta = display;
                }
            }
        }
        return melhorCarta;
    }

    public void TurnoDaIA()
    {
        jogoPausado = false; 
        if (maoAdversario.childCount == 0) return; 

        CardDisplay cartaEscolhida = null;
        string melhorAtributo = "";
        int maiorValor = -1;

        foreach (Transform filho in maoAdversario)
        {
            CardDisplay display = filho.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null)
            {
                CardData d = display.cardData;
                if (d.forca > maiorValor) { maiorValor = d.forca; melhorAtributo = "Força"; cartaEscolhida = display; }
                if (d.magia > maiorValor) { maiorValor = d.magia; melhorAtributo = "Magia"; cartaEscolhida = display; }
                if (d.agilidade > maiorValor) { maiorValor = d.agilidade; melhorAtributo = "Agilidade"; cartaEscolhida = display; }
                if (d.inteligencia > maiorValor) { maiorValor = d.inteligencia; melhorAtributo = "Inteligência"; cartaEscolhida = display; }
            }
        }

        if (cartaEscolhida != null)
        {
            cartaAtacanteIA = cartaEscolhida;
            atributoAtaqueIA = melhorAtributo;
            
            cartaEscolhida.transform.SetParent(canvasPrincipal.transform);
            cartaEscolhida.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
            cartaEscolhida.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            
            if (cartaEscolhida.imagemVerso != null) 
            {
                cartaEscolhida.imagemVerso.gameObject.SetActive(false);
                cartaEscolhida.AtualizarCarta();
            }
            if (textoAvisoIA != null)
            {
                textoAvisoIA.text = $"O Oponente atacou com: {melhorAtributo.ToUpper()}!";
                textoAvisoIA.gameObject.SetActive(true);
            }
        }
    }

    private int PegarValorAtributo(CardData carta, string atributo)
    {
        switch (atributo)
        {
            case "Força": return carta.forca;
            case "Magia": return carta.magia;
            case "Agilidade": return carta.agilidade;
            case "Inteligência": return carta.inteligencia;
            default: return 0;
        }
    }

    private void ResolverDuelo(CardDisplay cartaJogador, CardDisplay cartaOponente, string atributo)
    {
        cartaJogador.ResetarBonus();
        cartaOponente.ResetarBonus();

        if (cartaJogador.cardData.habilidadeEspecial != null)
        {
            cartaJogador.cardData.habilidadeEspecial.AtivarHabilidade(cartaJogador, cartaOponente);
        }

        if (cartaOponente.cardData.habilidadeEspecial != null)
        {
            cartaOponente.cardData.habilidadeEspecial.AtivarHabilidade(cartaOponente, cartaJogador);
        }

        int valorBaseJogador = PegarValorAtributo(cartaJogador.cardData, atributo);
        int valorBaseOponente = PegarValorAtributo(cartaOponente.cardData, atributo);

        int valorFinalJogador = valorBaseJogador + cartaJogador.valorTemporarioBonus;
        int valorFinalOponente = valorBaseOponente + cartaOponente.valorTemporarioBonus;

        if(cartaJogador.valorTemporarioBonus != 0 || cartaOponente.valorTemporarioBonus != 0)
        {
            Debug.Log($"Habilidades aplicadas! Jogador: {valorBaseJogador}->{valorFinalJogador} | IA: {valorBaseOponente}->{valorFinalOponente}");
        }

        if (efeitoChoque != null) efeitoChoque.Explodir();
        if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
        
        string mensagemDeCombate = "";

        if (valorFinalJogador > valorFinalOponente)
        {
            pontosJogador++;
            turnoDoJogador = true; 
            mensagemDeCombate = $"VITÓRIA!\n<size=50>{valorFinalJogador} x {valorFinalOponente}</size>";
        }
        else if (valorFinalOponente > valorFinalJogador)
        {
            pontosOponente++;
            turnoDoJogador = false; 
            mensagemDeCombate = $"DERROTA!\n<size=50>{valorFinalJogador} x {valorFinalOponente}</size>";
        }
        else 
        {
            int somaJogador = cartaJogador.cardData.forca + cartaJogador.cardData.magia + cartaJogador.cardData.agilidade + cartaJogador.cardData.inteligencia;
            int somaOponente = cartaOponente.cardData.forca + cartaOponente.cardData.magia + cartaOponente.cardData.agilidade + cartaOponente.cardData.inteligencia;

            if (somaJogador > somaOponente)
            {
                pontosJogador++;
                turnoDoJogador = true; 
                mensagemDeCombate = $"DESEMPATE (Soma Total)\nVITÓRIA!\n<size=40>{somaJogador} x {somaOponente}</size>";
            }
            else if (somaOponente > somaJogador)
            {
                pontosOponente++;
                turnoDoJogador = false; 
                mensagemDeCombate = $"DESEMPATE (Soma Total)\nDERROTA!\n<size=40>{somaJogador} x {somaOponente}</size>";
            }
            else 
            {
                mensagemDeCombate = $"EMPATE ABSOLUTO!\n<size=40>Até na soma ({somaJogador})</size>";
            }
        }

        AtualizarPlacares();
        jogoPausado = true;
        
        StartCoroutine(RotinaFimDeTurno(cartaJogador, cartaOponente, mensagemDeCombate));
    }

    private IEnumerator RotinaFimDeTurno(CardDisplay cartaJogador, CardDisplay cartaOponente, string mensagem)
    {
        if (textoResultado != null)
        {
            textoResultado.text = mensagem;
            textoResultado.gameObject.SetActive(true);
            textoResultado.transform.SetAsLastSibling(); 
        }

        yield return new WaitForSeconds(3.5f);

        if (textoResultado != null) textoResultado.gameObject.SetActive(false);
        
        EnviarParaCemiterio(cartaJogador, cemiterioJogador);
        EnviarParaCemiterio(cartaOponente, cemiterioOponente);

        cartaDoJogadorNaArena = null;
        cartaAtacanteIA = null;

        if (pontosJogador >= 3 || pontosOponente >= 3 || maoJogador.childCount == 0 || maoAdversario.childCount == 0)
        {
            StartCoroutine(EncerrarRodada());
        }
        else
        {
            jogoPausado = false; 
            if (!turnoDoJogador) TurnoDaIA(); 
        }
    }

    private IEnumerator EncerrarRodada()
    {
        jogoPausado = true;
        DesativarImagensDeResultado(); 
        
        if (pontosJogador > pontosOponente) { vitoriasJogador++; turnoDoJogador = true; }
        else if (pontosOponente > pontosJogador) { vitoriasOponente++; turnoDoJogador = false; }
        
        AtualizarPlacares();

        if (vitoriasJogador >= 3 || vitoriasOponente >= 3 || indiceCompra >= baralhoJogador.Count || indiceCompra >= baralhoOponente.Count)
        {
            FinalizarJogo();
            yield break; 
        }

        if (pontosJogador > pontosOponente) AtivarImagem(imgVitoriaRodada);
        else if (pontosOponente > pontosJogador) AtivarImagem(imgDerrotaRodada);
        else 
        { 
            if (imgEmpateRodada != null) AtivarImagem(imgEmpateRodada);
            else if (textoResultado != null) { textoResultado.text = "RODADA EMPATADA!"; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); }
        }
        
        yield return new WaitForSeconds(3f);
        
        DesativarImagensDeResultado(); 

        LimparMaos();
        yield return new WaitForSeconds(1f); 

        pontosJogador = 0;
        pontosOponente = 0;
        AtualizarPlacares();
        
        StartCoroutine(DistribuirCartasAnimado());
    }

    private void LimparMaos()
    {
        CardDisplay[] cartasJogador = maoJogador.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay carta in cartasJogador) EnviarParaCemiterio(carta, cemiterioJogador);

        CardDisplay[] cartasOponente = maoAdversario.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay carta in cartasOponente) EnviarParaCemiterio(carta, cemiterioOponente);
    }

    private void FinalizarJogo()
    {
        jogoPausado = true; 
        DesativarImagensDeResultado();
        
        Debug.Log($"Fim de Jogo. Placar: {vitoriasJogador} x {vitoriasOponente}");

        if (vitoriasJogador > vitoriasOponente) AtivarImagem(imgVitoriaJogo);
        else if (vitoriasOponente > vitoriasJogador) AtivarImagem(imgDerrotaJogo);
        else
        {
            if (imgEmpateJogo != null) AtivarImagem(imgEmpateJogo);
            else if (textoResultado != null) { textoResultado.text = "EMPATE TÉCNICO!"; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); }
        }

        StartCoroutine(RetornarAoMenuAposFim());
    }

    private IEnumerator RetornarAoMenuAposFim()
    {
        yield return new WaitForSeconds(4f); 
        SceneManager.LoadScene("MenuPrincipal"); 
    }

    private void AtivarImagem(Image img)
    {
        if (img != null)
        {
            if (img.transform.parent != null) 
            {
                img.transform.parent.gameObject.SetActive(true);
                img.transform.parent.SetAsLastSibling();
            }
            img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
        }
    }

    private void EnviarParaCemiterio(CardDisplay carta, Transform cemiterio)
    {
        if (carta == null || cemiterio == null) return;
        
        if (carta.imagemVerso != null) { carta.imagemVerso.gameObject.SetActive(false); carta.AtualizarCarta(); }
        
        carta.transform.SetParent(cemiterio);
        carta.transform.localPosition = Vector3.zero; 
        carta.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
        
        float giroAleatorio = Random.Range(-15f, 15f);
        carta.transform.localRotation = Quaternion.Euler(0, 0, giroAleatorio);

        CanvasGroup cg = carta.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;
    }

    public void AbrirCemiterio(bool ehCemiterioJogador)
    {
        if (painelVisualizadorCemiterio != null && painelVisualizadorCemiterio.activeSelf) return;

        FecharInspecao(); 
        jogoPausado = true;

        if (fundoInspecao != null) { fundoInspecao.SetActive(true); fundoInspecao.transform.SetAsLastSibling(); }
        if (painelVisualizadorCemiterio != null) { painelVisualizadorCemiterio.SetActive(true); painelVisualizadorCemiterio.transform.SetAsLastSibling(); }

        foreach (Transform filho in conteudoGradeCemiterio) Destroy(filho.gameObject);

        Transform cemiterioAlvo = ehCemiterioJogador ? cemiterioJogador : cemiterioOponente;

        foreach (Transform filho in cemiterioAlvo)
        {
            CardDisplay cartaNoCemiterio = filho.GetComponent<CardDisplay>();
            if (cartaNoCemiterio != null && cartaNoCemiterio.cardData != null)
            {
                GameObject novaCarta = Instantiate(prefabIconeCemiterio, conteudoGradeCemiterio);
                CardDisplay displayNovaCarta = novaCarta.GetComponent<CardDisplay>();
                if (displayNovaCarta != null)
                {
                    displayNovaCarta.cardData = cartaNoCemiterio.cardData;
                    displayNovaCarta.pertenceAoJogador = true; 
                    displayNovaCarta.AtualizarCarta();
                }
            }
        }
    }

    public void FecharInspecao()
    {
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);
        if (painelVisualizadorCemiterio != null) painelVisualizadorCemiterio.SetActive(false); 
        if (fundoInspecao != null) fundoInspecao.SetActive(false);
        jogoPausado = false; 
    }

    public void FecharCemiterio() { FecharInspecao(); }
    public void ClicarCemiterioJogador() { AbrirCemiterio(true); }
    public void ClicarCemiterioOponente() { AbrirCemiterio(false); }
    public void BotaoRenderSe() { SceneManager.LoadScene("MenuPrincipal"); }
}