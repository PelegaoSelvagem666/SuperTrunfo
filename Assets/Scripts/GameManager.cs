using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // <-- ADICIONADO: Necessário para usar Image
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Configurações do Baralho")]
    public List<CardData> baralhoCompleto;
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
    
    // --- NOVA SEÇÃO DE IMAGENS ---
    [Header("Imagens de Fim de Rodada/Jogo")]
    public Image imgVitoriaRodada;
    public Image imgDerrotaRodada;
    public Image imgEmpateRodada; // Opcional
    public Image imgVitoriaJogo;
    public Image imgDerrotaJogo;
    public Image imgEmpateJogo;   // Opcional
    // -----------------------------
    
    [Header("Placares Visuais")]
    public TextMeshProUGUI placarPontos;   
    public TextMeshProUGUI placarVitorias; 
    
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

        DesativarImagensDeResultado(); // Esconde as imagens de vitória/derrota ao iniciar

        vitoriasJogador = 0;
        vitoriasOponente = 0;
        pontosJogador = 0;
        pontosOponente = 0;
        indiceCompra = 0;
        AtualizarPlacares();

        EmbaralharBaralho();
        StartCoroutine(DistribuirCartasAnimado());
    }     

    // Função auxiliar para desligar todas as imagens da tela
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

    public void EmbaralharBaralho()
    {
        for (int i = 0; i < baralhoCompleto.Count; i++)
        {
            CardData temp = baralhoCompleto[i];
            int randomIndex = Random.Range(i, baralhoCompleto.Count);
            baralhoCompleto[i] = baralhoCompleto[randomIndex];
            baralhoCompleto[randomIndex] = temp;
        }
    }

    private System.Collections.IEnumerator DistribuirCartasAnimado()
    {
        for (int i = 0; i < 5; i++)
        {
            if (indiceCompra >= baralhoCompleto.Count) break;

            GameObject novaCartaJogador = Instantiate(cartaPrefab, maoJogador);
            CardDisplay displayJogador = novaCartaJogador.GetComponent<CardDisplay>();
            displayJogador.cardData = baralhoCompleto[indiceCompra];
            displayJogador.pertenceAoJogador = true;
            displayJogador.AtualizarCarta();
            indiceCompra++; 

            if (indiceCompra >= baralhoCompleto.Count) break;

            GameObject novaCartaAdversario = Instantiate(cartaPrefab, maoAdversario);
            CardDisplay displayAdversario = novaCartaAdversario.GetComponent<CardDisplay>();
            displayAdversario.cardData = baralhoCompleto[indiceCompra]; 
            displayAdversario.pertenceAoJogador = false;
            displayAdversario.AtualizarCarta();
            indiceCompra++; 

            yield return new WaitForSeconds(0.25f);
        }

        jogoPausado = false;

        if (!turnoDoJogador) 
        {
            TurnoDaIA();
        }
        else
        {
            Debug.Log("🎮 A RODADA COMEÇOU! É o seu turno de atacar.");
        }
    }

    private void AtualizarPlacares()
    {
        if (placarPontos != null) placarPontos.text = $"PONTOS (Rodada)\nVocê {pontosJogador} x {pontosOponente} IA";
        if (placarVitorias != null) placarVitorias.text = $"RODADAS VENCIDAS\nVocê {vitoriasJogador} x {vitoriasOponente} IA";
    }

    public void InspecionarCarta(CardData dados)
    {
        if (painelCartaDetalhe == null) return;

        // 1. Liga o fundo clicável (a parede de vidro escuro)
        if (fundoInspecao != null) 
        {
            fundoInspecao.SetActive(true);
            fundoInspecao.transform.SetAsLastSibling(); 
        }

        // 2. Puxa a mão do jogador para a FRENTE do fundo!
        if (maoJogador != null) maoJogador.SetAsLastSibling();

        // --- NOVIDADE AQUI: Puxa as cartas que estão no Campo/Arena para a frente ---
        if (cartaDoJogadorNaArena != null) cartaDoJogadorNaArena.transform.SetAsLastSibling();
        if (cartaAtacanteIA != null) cartaAtacanteIA.transform.SetAsLastSibling();

        // 3. Verifica o Cemitério
        // Se o visualizador já estiver aberto, ele fica na frente.
        if (painelVisualizadorCemiterio != null && painelVisualizadorCemiterio.activeSelf)
        {
            painelVisualizadorCemiterio.transform.SetAsLastSibling();
        }
        else
        {
            // Se estiver fechado, puxa os montinhos da mesa para permitir o clique único
            if (cemiterioJogador != null) cemiterioJogador.SetAsLastSibling(); 
            if (cemiterioOponente != null) cemiterioOponente.SetAsLastSibling(); 
        }

        // 4. Liga a carta detalhada por cima de TODOS os outros elementos
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
        int valorJogador = PegarValorAtributo(cartaJogador.cardData, atributo);
        int valorOponente = PegarValorAtributo(cartaOponente.cardData, atributo);
        
        if (efeitoChoque != null) efeitoChoque.Explodir();
        if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
        
        string mensagemDeCombate = "";

        if (valorJogador > valorOponente)
        {
            pontosJogador++;
            turnoDoJogador = true; 
            mensagemDeCombate = $"VITÓRIA!\n<size=50>{valorJogador} x {valorOponente}</size>";
        }
        else if (valorOponente > valorJogador)
        {
            pontosOponente++;
            turnoDoJogador = false; 
            mensagemDeCombate = $"DERROTA!\n<size=50>{valorJogador} x {valorOponente}</size>";
        }
        else
        {
            mensagemDeCombate = $"EMPATE!\n<size=50>Ambos com {valorJogador}</size>";
        }

        AtualizarPlacares();
        jogoPausado = true;
        
        StartCoroutine(RotinaFimDeTurno(cartaJogador, cartaOponente, mensagemDeCombate));
    }

    private System.Collections.IEnumerator RotinaFimDeTurno(CardDisplay cartaJogador, CardDisplay cartaOponente, string mensagem)
    {
        // Mostra o texto com os pontos (ex: 500 x 300) do duelo
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

   private System.Collections.IEnumerator EncerrarRodada()
    {
        jogoPausado = true;
        DesativarImagensDeResultado(); // Limpa a tela
        
        // 1. Atualiza as vitórias silenciosamente primeiro
        if (pontosJogador > pontosOponente) { vitoriasJogador++; turnoDoJogador = true; }
        else if (pontosOponente > pontosJogador) { vitoriasOponente++; turnoDoJogador = false; }
        
        AtualizarPlacares();

        // 2. CHECA SE O JOGO ACABOU AGORA (Antes de mostrar a imagem de rodada!)
        if (vitoriasJogador >= 3 || vitoriasOponente >= 3 || indiceCompra >= baralhoCompleto.Count)
        {
            FinalizarJogo();
            yield break; // Encerra essa rotina aqui mesmo, indo direto pro Game Over
        }

        // 3. SE O JOGO NÃO ACABOU, mostra a imagem de fim de rodada
        if (pontosJogador > pontosOponente) 
        { 
            if (imgVitoriaRodada != null) { 
                imgVitoriaRodada.gameObject.SetActive(true); 
                imgVitoriaRodada.transform.parent.SetAsLastSibling(); // Puxa o PainelResultados pra frente de tudo
                imgVitoriaRodada.transform.SetAsLastSibling(); 
            }
        }
        else if (pontosOponente > pontosJogador) 
        { 
            if (imgDerrotaRodada != null) { 
                imgDerrotaRodada.gameObject.SetActive(true); 
                imgDerrotaRodada.transform.parent.SetAsLastSibling(); 
                imgDerrotaRodada.transform.SetAsLastSibling(); 
            }
        }
        else 
        { 
            if (imgEmpateRodada != null) { 
                imgEmpateRodada.gameObject.SetActive(true); 
                imgEmpateRodada.transform.parent.SetAsLastSibling(); 
                imgEmpateRodada.transform.SetAsLastSibling(); 
            }
            else if (textoResultado != null) { 
                textoResultado.text = "RODADA EMPATADA!"; 
                textoResultado.gameObject.SetActive(true); 
                textoResultado.transform.SetAsLastSibling();
            }
        }
        
        // Espera a imagem ficar na tela
        yield return new WaitForSeconds(3f);
        
        DesativarImagensDeResultado(); // Esconde a imagem

        LimparMaos();
        yield return new WaitForSeconds(1f); 

        // Começa nova rodada
        pontosJogador = 0;
        pontosOponente = 0;
        AtualizarPlacares();
        StartCoroutine(DistribuirCartasAnimado());
    }
// NOVA FUNÇÃO: Joga tudo que sobrou na mão fora
    private void LimparMaos()
    {
        CardDisplay[] cartasJogador = maoJogador.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay carta in cartasJogador)
        {
            EnviarParaCemiterio(carta, cemiterioJogador);
        }

        CardDisplay[] cartasOponente = maoAdversario.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay carta in cartasOponente)
        {
            EnviarParaCemiterio(carta, cemiterioOponente);
        }
    }
    private void FinalizarJogo()
    {
        jogoPausado = true; 
        DesativarImagensDeResultado();
        
        // RAIO-X 1: Verifica se o código realmente chegou aqui e qual é o placar
        Debug.Log($"🚨 FIM DE JOGO ACIONADO! Placar Final -> Você: {vitoriasJogador} x {vitoriasOponente} IA");

        if (vitoriasJogador > vitoriasOponente)
        {
            // RAIO-X 2: Verifica se ele detectou a vitória
            Debug.Log("🏆 O código decidiu que VOCÊ VENCEU. Tentando ligar a imagem de Vitória...");
            AtivarImagem(imgVitoriaJogo);
        }
        else if (vitoriasOponente > vitoriasJogador)
        {
            // RAIO-X 3: Verifica se ele detectou a derrota
            Debug.Log("💀 O código decidiu que a IA VENCEU. Tentando ligar a imagem de Derrota...");
            AtivarImagem(imgDerrotaJogo);
        }
        else
        {
            Debug.Log("⚖️ O código decidiu que foi EMPATE.");
            if (imgEmpateJogo != null) AtivarImagem(imgEmpateJogo);
            else if (textoResultado != null) { textoResultado.text = "EMPATE TÉCNICO!"; textoResultado.gameObject.SetActive(true); textoResultado.transform.SetAsLastSibling(); }
        }
    }
// --- FUNÇÃO MÁGICA ---
    // Essa função garante que o painel pai e a imagem liguem e pulem pra frente de tudo
    private void AtivarImagem(Image img)
    {
        if (img != null)
        {
            // Puxa o painel pai pra frente de tudo e liga ele
            if (img.transform.parent != null) 
            {
                img.transform.parent.gameObject.SetActive(true);
                img.transform.parent.SetAsLastSibling();
            }
            
            // Liga a imagem em si
            img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
        }
    }
private void EnviarParaCemiterio(CardDisplay carta, Transform cemiterio)
    {
        if (carta == null || cemiterio == null) return;
        
        if (carta.imagemVerso != null) 
        {
            carta.imagemVerso.gameObject.SetActive(false);
            carta.AtualizarCarta();
        }
        
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
        // 1. TRAVA DE SEGURANÇA: Se o painel já estiver aberto, ignora cliques vazados
        if (painelVisualizadorCemiterio != null && painelVisualizadorCemiterio.activeSelf) return;

        // Limpa qualquer carta gigante da tela antes de abrir o cemitério
        FecharInspecao(); 

        jogoPausado = true;

        // 2. LIGA O FUNDO BLOQUEADOR: Impede de clicar no outro cemitério ou nas cartas
        if (fundoInspecao != null) 
        {
            fundoInspecao.SetActive(true);
            fundoInspecao.transform.SetAsLastSibling(); 
        }

        if (painelVisualizadorCemiterio != null)
        {
            painelVisualizadorCemiterio.SetActive(true);
            painelVisualizadorCemiterio.transform.SetAsLastSibling(); 
        }

        foreach (Transform filho in conteudoGradeCemiterio)
        {
            Destroy(filho.gameObject);
        }

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

    // --- FUNÇÃO QUE ESTAVA FALTANDO ---
    // Universal: Fecha a carta gigante, o cemitério e o fundo escuro de uma vez
    public void FecharInspecao()
    {
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);
        if (painelVisualizadorCemiterio != null) painelVisualizadorCemiterio.SetActive(false); 
        if (fundoInspecao != null) fundoInspecao.SetActive(false);
        
        jogoPausado = false; 
    }

    public void FecharCemiterio()
    {
        // Apenas chama a função universal de fechar
        FecharInspecao();
    }

    public void ClicarCemiterioJogador() { AbrirCemiterio(true); }
    public void ClicarCemiterioOponente() { AbrirCemiterio(false); }
}