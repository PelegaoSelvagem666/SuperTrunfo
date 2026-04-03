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

    [Header("Interface de Batalha (Opção C)")]
    public GameObject painelEscolhaAtributo; 
    
    [Tooltip("Arraste o grupo de hitboxes que fica dentro do Painel de Detalhes aqui!")]
    public GameObject grupoBotoesAtributoInspecao; 
    
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
    public bool habilidadeJaUsada = false;
    [Header("Seleção Manual de Habilidade")]
    public bool aguardandoSelecaoCemiterio = false;
    public CardDisplay cartaSelecionadaPeloEfeito = null;
    [Header("UI de Habilidades")]
    public GameObject botaoConfirmarSelecao; 
    [Header("UI de Confirmação Genérica")]
    public GameObject painelBotaoConfirmar;
    [HideInInspector] public bool aguardandoConfirmacao = false;
    [Header("Mecânica de Moeda")]
    public GameObject painelCaraOuCoroa;
    [HideInInspector] public string apostaMoedaAtual = "";
    private CardDisplay cartaSendoInspecionada; 
    [Header("Buffs para o Próximo Duelo")]
    public int promessaBuffVitoriaJogador = 0;   
    public int promessaBuffVitoriaOponente = 0; 
    public int buffProximaCartaJogador = 0;      
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
        if (grupoBotoesAtributoInspecao != null) grupoBotoesAtributoInspecao.SetActive(false);
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

        EmbaralharDeck(baralhoJogador);
        EmbaralharDeck(baralhoOponente);
        StartCoroutine(DistribuirCartasAnimado());
    } 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (painelConfiguracoes != null && painelConfiguracoes.activeSelf) FecharConfiguracoes();
            else if (fundoInspecao != null && fundoInspecao.activeSelf) FecharInspecao();
        }
    }

    private void CarregarDeckSalvoDoJogador()
    {
        baralhoJogador.Clear();
        CardData[] todasAsCartas = Resources.LoadAll<CardData>("Cartas");
        List<CardData> acervoCompleto = new List<CardData>(todasAsCartas);

        string idDeck = "1"; 
        if (GerenciadorDeDeck.instancia != null && !string.IsNullOrEmpty(GerenciadorDeDeck.instancia.deckIdAtual))
            idDeck = GerenciadorDeDeck.instancia.deckIdAtual;
        
        string chaveDoDeck = "MeuDeckSalvo_" + idDeck;

        if (PlayerPrefs.HasKey(chaveDoDeck))
        {
            string deckTexto = PlayerPrefs.GetString(chaveDoDeck);
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
        // 1. Calcula quantas cartas faltam para cada lado completar 5 na mão
        int faltamJogador = 5 - maoJogador.childCount;
        int faltamOponente = 5 - maoAdversario.childCount;

        // 2. O loop vai rodar apenas a quantidade máxima necessária
        int comprasNecessarias = Mathf.Max(faltamJogador, faltamOponente);

        for (int i = 0; i < comprasNecessarias; i++)
        {
            // Só compra se ainda faltar carta para o jogador
            if (i < faltamJogador && indiceCompra < baralhoJogador.Count)
            {
                GameObject novaCartaJogador = Instantiate(cartaPrefab, maoJogador);
                CardDisplay displayJogador = novaCartaJogador.GetComponent<CardDisplay>();
                displayJogador.cardData = baralhoJogador[indiceCompra];
                displayJogador.pertenceAoJogador = true;
                displayJogador.AtualizarCarta();
                Debug.Log($"[COMPRA] Jogador comprou a carta: {baralhoJogador[indiceCompra].nomeCarta}");
            }

            // Só compra se ainda faltar carta para o oponente
            if (i < faltamOponente && indiceCompra < baralhoOponente.Count)
            {
                GameObject novaCartaAdversario = Instantiate(cartaPrefab, maoAdversario);
                CardDisplay displayAdversario = novaCartaAdversario.GetComponent<CardDisplay>();
                displayAdversario.cardData = baralhoOponente[indiceCompra]; 
                displayAdversario.pertenceAoJogador = false;
                
                // Garante que a carta nova do oponente venha com o verso virado!
                if (displayAdversario.imagemVerso != null) displayAdversario.imagemVerso.gameObject.SetActive(true);
                
                displayAdversario.AtualizarCarta();
                Debug.Log($"[COMPRA] Oponente comprou uma carta escondida.");
            }

            // Avança no baralho (O seu sistema atual usa a mesma variável pros dois decks)
            indiceCompra++; 
            yield return new WaitForSeconds(0.25f);
        }

        jogoPausado = false;
        if (!turnoDoJogador) TurnoDaIA();
    }
    private void AtualizarPlacares()
    {
        if (txtPontosJogador != null) txtPontosJogador.text = pontosJogador.ToString();
        if (txtPontosOponente != null) txtPontosOponente.text = pontosOponente.ToString();
        if (txtVitoriasJogador != null) txtVitoriasJogador.text = vitoriasJogador.ToString();
        if (txtVitoriasOponente != null) txtVitoriasOponente.text = vitoriasOponente.ToString();
    }

    public void InspecionarCarta(CardDisplay cartaClicada) 
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
        
        painelCartaDetalhe.cardData = cartaClicada.cardData;                     
        painelCartaDetalhe.valorTemporarioBonus = cartaClicada.valorTemporarioBonus; 
        painelCartaDetalhe.AtualizarCarta();      
        cartaSendoInspecionada = cartaClicada; 

        if (aguardandoSelecaoCemiterio && (cartaClicada.transform.parent == cemiterioJogador || cartaClicada.transform.parent == cemiterioOponente || cartaClicada.transform.parent == conteudoGradeCemiterio))
        {
            if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(true);
        }
        else 
        {
            if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(false);
        }     

// --- A MÁGICA DA OPÇÃO C AQUI ---
        // Só acende os botões de ataque se for a carta da arena, for o turno do jogador 
        // E o jogo NÃO estiver pausado esperando o botão Continuar!
        if (cartaClicada == cartaDoJogadorNaArena && turnoDoJogador && !aguardandoConfirmacao)
        {
            if (grupoBotoesAtributoInspecao != null) 
            {
                grupoBotoesAtributoInspecao.SetActive(true);
                // Desfoca qualquer botão para evitar o "fantasma do teclado"
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
        else
        {
            // Se estivermos esperando o Continuar, os botões dourados ficam escondidos!
            if (grupoBotoesAtributoInspecao != null) grupoBotoesAtributoInspecao.SetActive(false);
        }
    } // <-- Fim da função InspecionarCarta

    public void ReceberCartaNaArena(CardDisplay cartaDoJogador) 
    {
        if (jogoPausado) return;
        cartaDoJogadorNaArena = cartaDoJogador; 

        cartaDoJogadorNaArena.valorTemporarioBonus = modificadorGlobalJogador + buffProximaCartaJogador;
        cartaDoJogadorNaArena.AtualizarCarta(); 

        StartCoroutine(RotinaEntradaNaArena(cartaDoJogadorNaArena));
    }

    private IEnumerator RotinaEntradaNaArena(CardDisplay carta)
    {
        if (carta.cardData.habilidadeEspecial != null)
        {
            yield return StartCoroutine(carta.cardData.habilidadeEspecial.AoEntrarEmCampoCoroutine(carta));
        }

        if (turnoDoJogador)
        {
           // Opção C: Auto-Inspeciona a carta para o jogador já clicar no status!
           InspecionarCarta(carta);
        }
        else
        {
          StartCoroutine(ResolverDuelo(cartaDoJogadorNaArena, cartaAtacanteIA, atributoAtaqueIA));
        }
    }

    // --- OS BOTÕES DE ESCOLHA FORAM ATUALIZADOS AQUI ---
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
        if (grupoBotoesAtributoInspecao != null) grupoBotoesAtributoInspecao.SetActive(false);
    }

   private void IniciarBatalha(string atributoEscolhido)
    {
        if (aguardandoConfirmacao) 
        {
            Debug.Log("Você precisa fechar a visão do Mago clicando em Continuar antes de atacar!");
            return;
        }
        
        if (grupoBotoesAtributoInspecao != null) grupoBotoesAtributoInspecao.SetActive(false);
        FecharInspecao();

        Debug.Log($"👤 TURNO DO JOGADOR: Você escolheu disputar {atributoEscolhido.ToUpper()}!");
        
        // Passa a bola para a corrotina para podermos ter tempo das animações
        StartCoroutine(RotinaIniciarBatalha(atributoEscolhido));
    }

    // NOVA FUNÇÃO:
    private IEnumerator RotinaIniciarBatalha(string atributoEscolhido)
    {
        CardDisplay cartaOponente = null;

        if (cartaAtacanteIA != null)
        {
            cartaOponente = cartaAtacanteIA;
        }
        else
        {
            cartaOponente = EscolherCartaDaIA(atributoEscolhido);
            
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

                // A MÁGICA: O Oponente dispara os efeitos antes de tomar o dano!
                if (cartaOponente.cardData.habilidadeEspecial != null)
                {
                    yield return StartCoroutine(cartaOponente.cardData.habilidadeEspecial.AoEntrarEmCampoCoroutine(cartaOponente));
                }
            }
        }

        if (cartaOponente != null)
        {
           yield return StartCoroutine(ResolverDuelo(cartaDoJogadorNaArena, cartaOponente, atributoEscolhido));
        }
    }

private CardDisplay EscolherCartaDaIA(string atributo)
    {
        if (maoAdversario.childCount == 0) return null;

        // Puxa a dificuldade direto da barrinha que você criou no Inspector (1 a 5)
        int dificuldade = oponenteAtual != null ? oponenteAtual.nivelDeDificuldade : 1;

        // =========================================================
        // NÍVEL 1: CAÓTICO (Pega qualquer carta aleatória e joga)
        // =========================================================
        if (dificuldade == 1)
        {
            int indexAleatorio = UnityEngine.Random.Range(0, maoAdversario.childCount);
            return maoAdversario.GetChild(indexAleatorio).GetComponent<CardDisplay>();
        }

        // --- PREPARAÇÃO DE DADOS PARA OS NÍVEIS 2 AO 5 ---
        int valorParaBater = PegarValorAtributo(cartaDoJogadorNaArena.cardData, atributo) + cartaDoJogadorNaArena.valorTemporarioBonus;
        
        List<CardDisplay> todasAsCartas = new List<CardDisplay>();
        List<CardDisplay> cartasQueGanham = new List<CardDisplay>();
        List<CardDisplay> cartasQuePerdem = new List<CardDisplay>();

        foreach (Transform filho in maoAdversario)
        {
            CardDisplay carta = filho.GetComponent<CardDisplay>();
            if (carta != null && carta.cardData != null)
            {
                todasAsCartas.Add(carta);
                if (PegarValorAtributo(carta.cardData, atributo) > valorParaBater) cartasQueGanham.Add(carta);
                else cartasQuePerdem.Add(carta);
            }
        }

        // Organiza as listas do mais FRACO para o mais FORTE calculando o poder geral da carta
        todasAsCartas.Sort((a, b) => CalcularPoderTotalDaCarta(a.cardData).CompareTo(CalcularPoderTotalDaCarta(b.cardData)));
        cartasQueGanham.Sort((a, b) => CalcularPoderTotalDaCarta(a.cardData).CompareTo(CalcularPoderTotalDaCarta(b.cardData)));
        cartasQuePerdem.Sort((a, b) => CalcularPoderTotalDaCarta(a.cardData).CompareTo(CalcularPoderTotalDaCarta(b.cardData)));

        // =========================================================
        // NÍVEL 2: INICIANTE / FORÇA BRUTA
        // =========================================================
        if (dificuldade == 2)
        {
            // Ordena pelo atributo específico da luta e joga a maior carta, sem ligar pro resto
            todasAsCartas.Sort((a, b) => PegarValorAtributo(a.cardData, atributo).CompareTo(PegarValorAtributo(b.cardData, atributo)));
            return todasAsCartas[todasAsCartas.Count - 1]; 
        }

        // =========================================================
        // NÍVEL 3: DESESPERADO
        // =========================================================
        if (dificuldade == 3)
        {
            // Se vai ganhar, usa a carta MAIS FORTE pra massacrar (Desperdício)
            if (cartasQueGanham.Count > 0) return cartasQueGanham[cartasQueGanham.Count - 1]; 
            // Se vai perder, joga a carta MAIS FORTE fora de pânico (Erro clássico)
            return cartasQuePerdem[cartasQuePerdem.Count - 1]; 
        }

        // =========================================================
        // NÍVEL 4 e 5: ESTRATEGISTA / MESTRE
        // =========================================================
        if (dificuldade >= 4)
        {
            // Eficiência Mínima: Usa a MAIS FRACA que ainda garante a vitória
            if (cartasQueGanham.Count > 0) return cartasQueGanham[0]; 
            // Sacrifício Tático: Já que vai perder, joga o LIXO fora e guarda os chefões
            return cartasQuePerdem[0]; 
        }

        return null;
    }

    public void TurnoDaIA()
    {
        jogoPausado = false; 
        if (maoAdversario.childCount == 0) return; 

        int dificuldade = oponenteAtual != null ? oponenteAtual.nivelDeDificuldade : 1;
        CardDisplay cartaEscolhida = null;
        string melhorAtributo = "Força";

        // =========================================================
        // ESCOLHA DA CARTA DE ATAQUE
        // =========================================================
        
        if (dificuldade == 1) // NÍVEL 1: CAÓTICO
        {
            int indexAleatorio = UnityEngine.Random.Range(0, maoAdversario.childCount);
            cartaEscolhida = maoAdversario.GetChild(indexAleatorio).GetComponent<CardDisplay>();
            string[] atributosPossiveis = { "Força", "Magia", "Agilidade", "Inteligência" };
            melhorAtributo = atributosPossiveis[UnityEngine.Random.Range(0, 4)];
        }
        else if (dificuldade == 2 || dificuldade == 3) // NÍVEL 2 e 3: FORÇA BRUTA
        {
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
        }
        else // NÍVEL 4 e 5: ESTRATEGISTA / MESTRE (Pico Polarizado)
        {
            int maiorPico = -1;
            foreach (Transform filho in maoAdversario)
            {
                CardDisplay display = filho.GetComponent<CardDisplay>();
                if (display != null && display.cardData != null)
                {
                    var (nomeAtrDaCarta, valorAtrDaCarta) = DescobrirMelhorAtributo(display.cardData);
                    if (valorAtrDaCarta > maiorPico)
                    {
                        maiorPico = valorAtrDaCarta;
                        melhorAtributo = nomeAtrDaCarta; 
                        cartaEscolhida = display;
                    }
                }
            }
        }

        // =========================================================
        // JOGA A CARTA NA MESA E INICIA O ATAQUE
        // =========================================================
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
            
            atributoEmDisputa = melhorAtributo;
            StartCoroutine(RotinaEntradaDaIA(cartaEscolhida, melhorAtributo));
        
        }
    }
    private IEnumerator RotinaEntradaDaIA(CardDisplay carta, string atributo)
    {
        // A MÁGICA: A IA finalmente usa sua habilidade "Ao Entrar"!
        if (carta.cardData.habilidadeEspecial != null)
        {
            yield return StartCoroutine(carta.cardData.habilidadeEspecial.AoEntrarEmCampoCoroutine(carta));
        }

        atributoEmDisputa = atributo;
        if (textoAvisoIA != null)
        {
            textoAvisoIA.text = $"O Oponente atacou com: {atributo.ToUpper()}!";
            textoAvisoIA.gameObject.SetActive(true);
        }
    }

public int PegarValorAtributo(CardData carta, string atributo)
    {
        // O ToLower() transforma tudo em minúsculo antes de testar
        switch (atributo.ToLower())
        {
            case "força":
            case "forca": 
                return carta.forca;
            case "magia": 
                return carta.magia;
            case "agilidade": 
                return carta.agilidade;
            case "inteligência":
            case "inteligencia": 
                return carta.inteligencia;
            default: 
                return 0; // Se chegar aqui de novo, deu algo muito errado!
        }
    }

  private IEnumerator ResolverDuelo(CardDisplay cartaJogador, CardDisplay cartaOponente, string atributo)
    {
        atributoEmDisputa = atributo;
        cartaJogador.ResetarBonus();
        cartaOponente.ResetarBonus();

        cartaJogador.valorTemporarioBonus += modificadorGlobalJogador + buffProximaCartaJogador;
        cartaOponente.valorTemporarioBonus += modificadorGlobalOponente + buffProximaCartaOponente;
        
        buffProximaCartaJogador = 0;
        buffProximaCartaOponente = 0;

        CardDisplay primeiroAAtivar = turnoDoJogador ? cartaJogador : cartaOponente; 
        CardDisplay segundoAAtivar = turnoDoJogador ? cartaOponente : cartaJogador; 
        if (primeiroAAtivar.cardData.habilidadeEspecial != null)
            yield return StartCoroutine(primeiroAAtivar.cardData.habilidadeEspecial.AtivarHabilidadeCoroutine(primeiroAAtivar, segundoAAtivar));

        if (segundoAAtivar.cardData.habilidadeEspecial != null)
            yield return StartCoroutine(segundoAAtivar.cardData.habilidadeEspecial.AtivarHabilidadeCoroutine(segundoAAtivar, primeiroAAtivar));

        if (interrupcaoDeHabilidade)
        {
            interrupcaoDeHabilidade = false;
            yield break; 
        }
        cartaJogador.AtualizarCarta();
        cartaOponente.AtualizarCarta();
    
        int valorFinalJogador = Mathf.Clamp(PegarValorAtributo(cartaJogador.cardData, atributo) + cartaJogador.valorTemporarioBonus, 0, 1000);
        int valorFinalOponente = Mathf.Clamp(PegarValorAtributo(cartaOponente.cardData, atributo) + cartaOponente.valorTemporarioBonus, 0, 1000);

        if (efeitoChoque != null) efeitoChoque.Explodir();
        if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
        
        string mensagemDeCombate = "";
        // --- INÍCIO DO RELATÓRIO DO CONSOLE ---
        int valorBaseJogador = PegarValorAtributo(cartaJogador.cardData, atributoEmDisputa);
        int bonusJogador = cartaJogador.valorTemporarioBonus;
        int totalJogador = valorBaseJogador + bonusJogador;

        int valorBaseOponente = PegarValorAtributo(cartaOponente.cardData, atributoEmDisputa);
        int bonusOponente = cartaOponente.valorTemporarioBonus;
        int totalOponente = valorBaseOponente + bonusOponente;

        Debug.Log("<color=yellow>=== ⚔️ RELATÓRIO DE BATALHA ⚔️ ===</color>");
        Debug.Log($"Disputa de Atributo: <b>{atributoEmDisputa.ToUpper()}</b>");
        
        // Log do Jogador
        string sinalJog = bonusJogador >= 0 ? "+" : ""; 
        Debug.Log($"<color=cyan>👤 JOGADOR:</color> {cartaJogador.cardData.nomeCarta} | Base: {valorBaseJogador} | Modificadores: {sinalJog}{bonusJogador} | <b>TOTAL: {totalJogador}</b>");

        // Log do Oponente
        string sinalOp = bonusOponente >= 0 ? "+" : "";
        Debug.Log($"<color=red>🤖 OPONENTE:</color> {cartaOponente.cardData.nomeCarta} | Base: {valorBaseOponente} | Modificadores: {sinalOp}{bonusOponente} | <b>TOTAL: {totalOponente}</b>");

        // Resultado
        if (totalJogador > totalOponente) Debug.Log("<color=green>🏆 VENCEDOR: JOGADOR</color>");
        else if (totalOponente > totalJogador) Debug.Log("<color=orange>💀 VENCEDOR: OPONENTE</color>");
        else Debug.Log("<color=white>🤝 RESULTADO: EMPATE</color>");
        
        Debug.Log("<color=yellow>==================================</color>");
        // --- FIM DO RELATÓRIO DO CONSOLE ---

        if (valorFinalJogador > valorFinalOponente)
        {
            pontosJogador++;
            turnoDoJogador = true; 
            mensagemDeCombate = $"VITÓRIA!\n<size=50>{valorFinalJogador} x {valorFinalOponente}</size>";
            if (promessaBuffVitoriaJogador > 0) buffProximaCartaJogador = promessaBuffVitoriaJogador;
        }
        else if (valorFinalOponente > valorFinalJogador)
        {
            pontosOponente++;
            turnoDoJogador = false; 
            mensagemDeCombate = $"DERROTA!\n<size=50>{valorFinalJogador} x {valorFinalOponente}</size>";
            if (promessaBuffVitoriaOponente > 0) buffProximaCartaOponente = promessaBuffVitoriaOponente;
        }
        else 
        {
            int somaJogador = cartaJogador.cardData.forca + cartaJogador.cardData.magia + cartaJogador.cardData.agilidade + cartaJogador.cardData.inteligencia;
            int somaOponente = cartaOponente.cardData.forca + cartaOponente.cardData.magia + cartaOponente.cardData.agilidade + cartaOponente.cardData.inteligencia;

            if (somaJogador > somaOponente) { pontosJogador++; turnoDoJogador = true; mensagemDeCombate = $"DESEMPATE (Soma Total)\nVITÓRIA!\n<size=40>{somaJogador} x {somaOponente}</size>"; }
            else if (somaOponente > somaJogador) { pontosOponente++; turnoDoJogador = false; mensagemDeCombate = $"DESEMPATE (Soma Total)\nDERROTA!\n<size=40>{somaJogador} x {somaOponente}</size>"; }
            else { mensagemDeCombate = $"EMPATE ABSOLUTO!\n<size=40>Até na soma ({somaJogador})</size>"; }
        }

        promessaBuffVitoriaJogador = 0;
        promessaBuffVitoriaOponente = 0;
        
        AtualizarPlacares();
        jogoPausado = true;
        
        StartCoroutine(RotinaFimDeTurno(cartaJogador, cartaOponente, mensagemDeCombate));
    }

    private IEnumerator RotinaFimDeTurno(CardDisplay cartaJogador, CardDisplay cartaOponente, string mensagem)
    {
        if (textoResultado != null) { textoResultado.text = mensagem; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); }
        yield return new WaitForSeconds(3.5f);
        if (textoResultado != null) textoResultado.gameObject.SetActive(false);
        
        habilidadeJaUsada = false; 
        interrupcaoDeHabilidade = false;
        EnviarParaCemiterio(cartaJogador, cemiterioJogador);
        EnviarParaCemiterio(cartaOponente, cemiterioOponente);

        cartaDoJogadorNaArena = null;
        cartaAtacanteIA = null;

        if (pontosJogador >= 3 || pontosOponente >= 3 || maoJogador.childCount == 0 || maoAdversario.childCount == 0) StartCoroutine(EncerrarRodada());
        else { jogoPausado = false; if (!turnoDoJogador) TurnoDaIA(); }
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
        else { if (imgEmpateRodada != null) AtivarImagem(imgEmpateRodada); else if (textoResultado != null) { textoResultado.text = "RODADA EMPATADA!"; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); } }
        
        yield return new WaitForSeconds(3f);
        DesativarImagensDeResultado(); 
        //LimparMaos();
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
        if (vitoriasJogador > vitoriasOponente) AtivarImagem(imgVitoriaJogo);
        else if (vitoriasOponente > vitoriasJogador) AtivarImagem(imgDerrotaJogo);
        else { if (imgEmpateJogo != null) AtivarImagem(imgEmpateJogo); else if (textoResultado != null) { textoResultado.text = "EMPATE TÉCNICO!"; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); } }
        StartCoroutine(RetornarAoMenuAposFim());
    }

    private IEnumerator RetornarAoMenuAposFim() { yield return new WaitForSeconds(4f); SceneManager.LoadScene("MenuPrincipal"); }

    private void AtivarImagem(Image img) { if (img != null) { if (img.transform.parent != null) { img.transform.parent.gameObject.SetActive(true); img.transform.parent.SetAsLastSibling(); } img.gameObject.SetActive(true); img.transform.SetAsLastSibling(); } }

    private void EnviarParaCemiterio(CardDisplay carta, Transform cemiterio)
    {
        if (carta == null || cemiterio == null) return;
        carta.ResetarBonus();
        if (carta.imagemVerso != null) carta.imagemVerso.gameObject.SetActive(false); 
        carta.AtualizarCarta();
        carta.transform.SetParent(cemiterio);
        carta.transform.localPosition = Vector3.zero; 
        carta.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
        carta.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
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
                if (displayNovaCarta != null) { displayNovaCarta.cardData = cartaNoCemiterio.cardData; displayNovaCarta.pertenceAoJogador = true; displayNovaCarta.AtualizarCarta(); }
            }
        }
    }

    public void FecharInspecao()
    {
        // Se o jogador cancelar fechando o painel de inspeção sem escolher o ataque
        if (grupoBotoesAtributoInspecao != null && grupoBotoesAtributoInspecao.activeSelf)
        {
            CancelarJogada();
        }

        if (grupoBotoesAtributoInspecao != null) grupoBotoesAtributoInspecao.SetActive(false);
        if (painelCartaDetalhe != null) { painelCartaDetalhe.valorTemporarioBonus = 0; painelCartaDetalhe.gameObject.SetActive(false); }
        if (painelVisualizadorCemiterio != null) painelVisualizadorCemiterio.SetActive(false); 
        if (fundoInspecao != null) fundoInspecao.SetActive(false);
        jogoPausado = false; 
        if (botaoConfirmarSelecao != null) botaoConfirmarSelecao.SetActive(false);
        cartaSendoInspecionada = null;
    }

    public void ConfirmarSelecaoCemiterio()
    {
        if (aguardandoSelecaoCemiterio && cartaSendoInspecionada != null)
        {
            cartaSelecionadaPeloEfeito = cartaSendoInspecionada; 
            aguardandoSelecaoCemiterio = false;                  
            FecharInspecao(); 
        }
    }

    public void ForcarVezDeEscolha(bool ehVezDoPlayer)
    {
        if (habilidadeJaUsada) return; 
        habilidadeJaUsada = true;       
        interrupcaoDeHabilidade = true; 
        turnoDoJogador = ehVezDoPlayer; 
        
        if (turnoDoJogador)
        {
            // O jogador é forçado a escolher, abrimos a carta da arena!
            if (cartaDoJogadorNaArena != null) InspecionarCarta(cartaDoJogadorNaArena);
            if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
        }
    }

    public void FecharCemiterio() { FecharInspecao(); }
    public void ClicarCemiterioJogador() { AbrirCemiterio(true); }
    public void ClicarCemiterioOponente() { AbrirCemiterio(false); }
    [Header("Configurações")]
    public GameObject painelConfiguracoes;
    public void AbrirConfiguracoes() { if (painelConfiguracoes != null) { painelConfiguracoes.SetActive(true); painelConfiguracoes.transform.SetAsLastSibling(); } }
    public void FecharConfiguracoes() { if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false); }
    public void BotaoRenderSe() { SceneManager.LoadScene("MenuPrincipal"); }


    public void ForcarTrocaDeCartaAdversario(CardDisplay cartaAntiga)
    {
        StartCoroutine(RotinaTrocaCartaAdversario(cartaAntiga));
    }

    private IEnumerator RotinaTrocaCartaAdversario(CardDisplay cartaAntiga)
    {
        // 1. Tira a carta antiga do caminho e esconde (para a IA não escolher ela de novo por acidente)
        cartaAntiga.gameObject.SetActive(false); 

        // 2. Aviso dramático na tela para o jogador entender o que houve
        if (textoAvisoIA != null) 
        {
            textoAvisoIA.text = "Ataque Repelido!\nO oponente foi forçado a trocar de carta!";
            textoAvisoIA.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(2.5f);

        // 3. A IA vasculha a mão e escolhe uma NOVA carta baseada no MESMO atributo da antiga
        CardDisplay novaCarta = EscolherCartaDaIA(atributoEmDisputa);

        // 4. Agora sim, devolve a carta original para a mão do oponente com segurança
        cartaAntiga.transform.SetParent(maoAdversario, false);
        cartaAntiga.ResetarBonus();
        if (cartaAntiga.imagemVerso != null) cartaAntiga.imagemVerso.gameObject.SetActive(true);
        cartaAntiga.gameObject.SetActive(true);
        cartaAntiga.AtualizarCarta();

        if (novaCarta != null)
        {
            cartaAtacanteIA = novaCarta; // Atualiza quem é o novo lutador
            
            // 5. Posiciona a nova carta na arena com efeito
            novaCarta.transform.SetParent(canvasPrincipal.transform);
            novaCarta.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
            novaCarta.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            novaCarta.valorTemporarioBonus = modificadorGlobalOponente + buffProximaCartaOponente;

            if (novaCarta.imagemVerso != null) 
            {
                novaCarta.imagemVerso.gameObject.SetActive(false);
                novaCarta.AtualizarCarta();
            }

            if (textoAvisoIA != null) textoAvisoIA.text = $"Novo ataque: {atributoEmDisputa.ToUpper()}!";
            yield return new WaitForSeconds(1.5f);
            
            // 6. Reinicia a porradaria com o novo oponente!
            StartCoroutine(ResolverDuelo(cartaDoJogadorNaArena, novaCarta, atributoEmDisputa));
        }
    }
    // --- LÓGICA DA MOEDA ---
    public void EscolherCara()
    {
        apostaMoedaAtual = "Cara";
    }

    public void EscolherCoroa()
    {
        apostaMoedaAtual = "Coroa";
    }
    // --- LÓGICA DE PAUSA DA HABILIDADE ---
    public void ConfirmarAcao()
    {
        aguardandoConfirmacao = false;
    }
    // --- CÉREBRO DA IA: FERRAMENTAS ---
    
    // Calcula o poder bruto da carta (útil para saber se a carta é valiosa ou um "lixo" a ser sacrificado)
    private int CalcularPoderTotalDaCarta(CardData carta)
    {
        return carta.forca + carta.magia + carta.agilidade + carta.inteligencia;
    }

    // Lê o valor exato do atributo que está sendo disputado
    private int PegarValorDoAtributo(CardData carta, string atributo)
    {
        switch (atributo.ToLower())
        {
            case "forca": return carta.forca;
            case "magia": return carta.magia;
            case "agilidade": return carta.agilidade;
            case "inteligencia": return carta.inteligencia;
            default: return 0;
        }
    }

    // Acha qual é o melhor atributo que uma carta tem e retorna o nome dele e o valor
    private (string, int) DescobrirMelhorAtributo(CardData carta)
    {
        string melhor = "Força";
        int maiorValor = carta.forca;

        if (carta.magia > maiorValor) { maiorValor = carta.magia; melhor = "Magia"; }
        if (carta.agilidade > maiorValor) { maiorValor = carta.agilidade; melhor = "Agilidade"; }
        if (carta.inteligencia > maiorValor) { maiorValor = carta.inteligencia; melhor = "Inteligencia"; }

        return (melhor, maiorValor);
    }
}