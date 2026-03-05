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

    [Header("Informação Pública de Batalha")]
    public string atributoEmDisputa; 

    [Header("Modificadores Globais da Rodada")]
    public int modificadorGlobalJogador = 0;
    public int modificadorGlobalOponente = 0;

    [Header("Controle de Habilidades")]
    public bool interrupcaoDeHabilidade = false; 
    private bool habilidadeJaUsada = false;
    [Header("Seleção Manual de Habilidade")]
    public bool aguardandoSelecaoCemiterio = false;
    public CardDisplay cartaSelecionadaPeloEfeito = null;
    [Header("UI de Habilidades")]
    public GameObject botaoConfirmarSelecao; 
    private CardDisplay cartaSendoInspecionada; // Memória para saber qual carta está no painel
    [Header("Buffs para o Próximo Duelo")]
    public int promessaBuffVitoriaJogador = 0;   // Fica guardado aguardando o resultado
    public int promessaBuffVitoriaOponente = 0; 
    public int buffProximaCartaJogador = 0;      // O buff real que vai ser aplicado
    public int buffProximaCartaOponente = 0;
    
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
        modificadorGlobalJogador = 0;
        modificadorGlobalOponente = 0;
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
        if (oponenteAtual != null)
        {
            Debug.Log($"⚔️ BATALHA INICIADA! Jogador ({baralhoJogador.Count} cartas) VS {oponenteAtual.nomeDoBot} ({baralhoOponente.Count} cartas)");
        }
        StartCoroutine(DistribuirCartasAnimado());
    } 
    void Update()
    {
        // Se apertar a tecla ESC (Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Tenta fechar as Configurações primeiro
            if (painelConfiguracoes != null && painelConfiguracoes.activeSelf)
            {
                FecharConfiguracoes();
            }
            // Se as configurações não estiverem abertas, tenta fechar a tela de Inspeção/Cemitério
            else if (fundoInspecao != null && fundoInspecao.activeSelf)
            {
                FecharInspecao();
            }
        }
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

    public void InspecionarCarta(CardDisplay cartaClicada) // <--- Agora recebe o CardDisplay
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
        
        // --- A MÁGICA ACONTECE AQUI ---
        painelCartaDetalhe.cardData = cartaClicada.cardData;                     // Copia a arte e atributos originais
        painelCartaDetalhe.valorTemporarioBonus = cartaClicada.valorTemporarioBonus; // Copia os buffs/nerfs atuais!
        painelCartaDetalhe.AtualizarCarta();      
        cartaSendoInspecionada = cartaClicada; // Salva a referência real da carta

        // Se o jogo está pausado pedindo uma carta, E a carta clicada for de algum cemitério:
        if (aguardandoSelecaoCemiterio && (cartaClicada.transform.parent == cemiterioJogador || cartaClicada.transform.parent == cemiterioOponente || cartaClicada.transform.parent == conteudoGradeCemiterio))
        {
            if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(true);
        }
        else // Inspeção normal, esconde o botão
        {
            if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(false);
        }                               // Repinta os textos com as cores certas
    }

    public void ReceberCartaNaArena(CardDisplay cartaDoJogador) 
    {
        if (jogoPausado) return;
        cartaDoJogadorNaArena = cartaDoJogador; 

        cartaDoJogadorNaArena.valorTemporarioBonus = modificadorGlobalJogador + buffProximaCartaJogador;
        cartaDoJogadorNaArena.AtualizarCarta(); 

        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);

        // A MÁGICA ACONTECE AQUI: Inicia uma rotina em vez de abrir o painel direto
        StartCoroutine(RotinaEntradaNaArena(cartaDoJogadorNaArena));
    }

    // --- NOVA ROTINA DE ENTRADA ---
    private IEnumerator RotinaEntradaNaArena(CardDisplay carta)
    {
        // 1. O jogo pausa aqui e resolve a habilidade no cemitério primeiro!
        if (carta.cardData.habilidadeEspecial != null)
        {
            yield return StartCoroutine(carta.cardData.habilidadeEspecial.AoEntrarEmCampoCoroutine(carta));
        }

        // 2. Depois que a habilidade acabou (você clicou no Confirmar), o jogo continua!
        if (turnoDoJogador)
        {
           if (painelEscolhaAtributo != null) 
            {
                painelEscolhaAtributo.SetActive(true);
                painelEscolhaAtributo.transform.SetAsLastSibling();
            }
        }
        else
        {
            // Se você estava defendendo, a IA já escolheu o atributo, então vai direto pra luta
          StartCoroutine(ResolverDuelo(cartaDoJogadorNaArena, cartaAtacanteIA, atributoEmDisputa));
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
            cartaDoJogadorNaArena.ResetarBonus();
            cartaDoJogadorNaArena.transform.SetParent(maoJogador, false);
            cartaDoJogadorNaArena = null; 
        }
        if (painelEscolhaAtributo != null) painelEscolhaAtributo.SetActive(false);
    }

    private void IniciarBatalha(string atributoEscolhido)
    {
        painelEscolhaAtributo.SetActive(false);
        Debug.Log($"👤 TURNO DO JOGADOR: Você escolheu disputar {atributoEscolhido.ToUpper()}!");
        CardDisplay cartaOponente = null;

        // CASO SERJÃO: Se a IA já tinha atacado, a carta dela já está guardada aqui.
        // Usamos ela mesma em vez de pedir uma nova.
        if (cartaAtacanteIA != null)
        {
            cartaOponente = cartaAtacanteIA;
        }
        else
        {
            // JOGO NORMAL: A IA escolhe uma carta da mão baseada no atributo
            cartaOponente = EscolherCartaDaIA(atributoEscolhido);
            
            // Só movemos visualmente se ela veio da mão agora
            if (cartaOponente != null)
            {
                cartaOponente.transform.SetParent(canvasPrincipal.transform);
                cartaOponente.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
                cartaOponente.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
                cartaOponente.valorTemporarioBonus = modificadorGlobalOponente + buffProximaCartaOponente;

                if (cartaOponente.imagemVerso != null)
                {
                    cartaOponente.imagemVerso.gameObject.SetActive(false);
                    cartaOponente.AtualizarCarta(); 
                }
            }
        }

        if (cartaOponente != null)
        {
           StartCoroutine(ResolverDuelo(cartaDoJogadorNaArena, cartaOponente, atributoEscolhido));
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
            cartaEscolhida.valorTemporarioBonus = modificadorGlobalOponente + buffProximaCartaOponente;

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
            Debug.Log($"🤖 TURNO DA IA: O Oponente jogou '{cartaEscolhida.cardData.nomeCarta}' e disputará {melhorAtributo.ToUpper()}!");
        }
    }

    public int PegarValorAtributo(CardData carta, string atributo)
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

  private IEnumerator ResolverDuelo(CardDisplay cartaJogador, CardDisplay cartaOponente, string atributo)
    {
        atributoEmDisputa = atributo;
        
        cartaJogador.ResetarBonus();
        cartaOponente.ResetarBonus();

        // 1. APLICA TODOS OS BUFFS E MALDIÇÕES JUNTOS AQUI:
        cartaJogador.valorTemporarioBonus += modificadorGlobalJogador + buffProximaCartaJogador;
        cartaOponente.valorTemporarioBonus += modificadorGlobalOponente + buffProximaCartaOponente;
        
        // Zera o buff da "próxima carta" para ele não durar para sempre
        buffProximaCartaJogador = 0;
        buffProximaCartaOponente = 0;
        Debug.Log($"⚔️ EMBATE: {cartaJogador.cardData.nomeCarta} VS {cartaOponente.cardData.nomeCarta} (Atributo: {atributo.ToUpper()})");

        CardDisplay primeiroAAtivar = turnoDoJogador ? cartaJogador : cartaOponente; // Atacante
        CardDisplay segundoAAtivar = turnoDoJogador ? cartaOponente : cartaJogador; // Defensor
        if (primeiroAAtivar.cardData.habilidadeEspecial != null)
            yield return StartCoroutine(primeiroAAtivar.cardData.habilidadeEspecial.AtivarHabilidadeCoroutine(primeiroAAtivar, segundoAAtivar));

        if (segundoAAtivar.cardData.habilidadeEspecial != null)
            yield return StartCoroutine(segundoAAtivar.cardData.habilidadeEspecial.AtivarHabilidadeCoroutine(segundoAAtivar, primeiroAAtivar));

        // Freio do Serjão Berranteiro
        if (interrupcaoDeHabilidade)
        {
            interrupcaoDeHabilidade = false;
            yield break; 
        }
        cartaJogador.AtualizarCarta();
        cartaOponente.AtualizarCarta();
    
        int valorBaseJogador = PegarValorAtributo(cartaJogador.cardData, atributo);
        int valorBaseOponente = PegarValorAtributo(cartaOponente.cardData, atributo);

        // Limita entre 0 e 1000
        int valorFinalJogador = Mathf.Clamp(valorBaseJogador + cartaJogador.valorTemporarioBonus, 0, 1000);
        int valorFinalOponente = Mathf.Clamp(valorBaseOponente + cartaOponente.valorTemporarioBonus, 0, 1000);

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
            Debug.Log($"🏆 RESULTADO: O Jogador amassou! ({valorFinalJogador} x {valorFinalOponente})");
            
            // ✅ SE O JOGADOR VENCEU, CUMPRE A PROMESSA DE BUFF:
            if (promessaBuffVitoriaJogador > 0) buffProximaCartaJogador = promessaBuffVitoriaJogador;
        }
        else if (valorFinalOponente > valorFinalJogador)
        {
            pontosOponente++;
            turnoDoJogador = false; 
            mensagemDeCombate = $"DERROTA!\n<size=50>{valorFinalJogador} x {valorFinalOponente}</size>";
            Debug.Log($"💀 RESULTADO: A IA levou a melhor... ({valorFinalOponente} x {valorFinalJogador})");

            // ✅ SE O OPONENTE VENCEU, CUMPRE A PROMESSA DE BUFF:
            if (promessaBuffVitoriaOponente > 0) buffProximaCartaOponente = promessaBuffVitoriaOponente;
        }
        else 
        {
            // EMPATE
            int somaJogador = cartaJogador.cardData.forca + cartaJogador.cardData.magia + cartaJogador.cardData.agilidade + cartaJogador.cardData.inteligencia;
            int somaOponente = cartaOponente.cardData.forca + cartaOponente.cardData.magia + cartaOponente.cardData.agilidade + cartaOponente.cardData.inteligencia;

            if (somaJogador > somaOponente)
            {
                pontosJogador++;
                turnoDoJogador = true; 
                mensagemDeCombate = $"DESEMPATE (Soma Total)\nVITÓRIA!\n<size=40>{somaJogador} x {somaOponente}</size>";
                Debug.Log($"⚖️ RESULTADO: Empate no atributo principal!");
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

        // 🧹 LIMPEZA DAS PROMESSAS FICA AQUI FORA DOS IFS!
        promessaBuffVitoriaJogador = 0;
        promessaBuffVitoriaOponente = 0;
        
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
        habilidadeJaUsada = false; 
        interrupcaoDeHabilidade = false;
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
        buffProximaCartaJogador = 0;
        AtualizarPlacares();

        modificadorGlobalJogador = 0;
        modificadorGlobalOponente = 0;

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
        
        carta.ResetarBonus();

        // 1º PASSO: Tira a capa roxa PRIMEIRO
        if (carta.imagemVerso != null) 
        { 
            carta.imagemVerso.gameObject.SetActive(false); 
        }
        
        // 2º PASSO: Agora sim manda atualizar (como ela não tem capa, vai desenhar a arte)
        carta.AtualizarCarta();

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
        if (painelCartaDetalhe != null) 
        {
            painelCartaDetalhe.valorTemporarioBonus = 0; // <--- Limpa os bônus do painel ao fechar
            painelCartaDetalhe.gameObject.SetActive(false);
        }
        if (painelVisualizadorCemiterio != null) painelVisualizadorCemiterio.SetActive(false); 
        if (fundoInspecao != null) fundoInspecao.SetActive(false);
        jogoPausado = false; 
        if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(false);
        cartaSendoInspecionada = null;
    }
    // Esta é a função que o botão "CONFIRMAR ESCOLHA" vai chamar ao ser clicado!
    public void ConfirmarSelecaoCemiterio()
    {
        if (aguardandoSelecaoCemiterio && cartaSendoInspecionada != null)
        {
            cartaSelecionadaPeloEfeito = cartaSendoInspecionada; // Envia a carta para a Habilidade
            aguardandoSelecaoCemiterio = false;                  // Destrava a batalha!
            
            FecharInspecao(); // Esconde a tela de detalhes e o botão automaticamente
        }
    }
    public void ForcarVezDeEscolha(bool ehVezDoPlayer)
    {
        // Se já usamos a habilidade nesta rodada, não faz de novo (evita loop infinito)
        if (habilidadeJaUsada) return; 

        habilidadeJaUsada = true;       // Marca que já usou
        interrupcaoDeHabilidade = true; // SINAL VERMELHO: Manda o ResolverDuelo parar!
        
        turnoDoJogador = ehVezDoPlayer; 
        
        if (turnoDoJogador)
        {
            if (painelEscolhaAtributo != null) 
            {
                painelEscolhaAtributo.SetActive(true);
                painelEscolhaAtributo.transform.SetAsLastSibling();
            }
            // Esconde o aviso "Oponente atacou com Magia", já que você anulou isso
            if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
            
            Debug.Log("Habilidade do Serjão: Duelo interrompido para nova escolha!");
        }
    }

    public void FecharCemiterio() { FecharInspecao(); }
    public void ClicarCemiterioJogador() { AbrirCemiterio(true); }
    public void ClicarCemiterioOponente() { AbrirCemiterio(false); }
    [Header("Configurações")]
    public GameObject painelConfiguracoes;

    public void AbrirConfiguracoes() 
    { 
        if (painelConfiguracoes != null) 
        {
            painelConfiguracoes.SetActive(true);
            painelConfiguracoes.transform.SetAsLastSibling(); // Joga pra frente de tudo
        }
    }
    
    public void FecharConfiguracoes() 
    { 
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false); 
    }
    public void BotaoRenderSe() { SceneManager.LoadScene("MenuPrincipal"); }
}