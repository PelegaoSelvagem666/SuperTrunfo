using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Configurações do Baralho")]
    public List<CardData> baralhoCompleto;
    public GameObject cartaPrefab;
    private int indiceCompra = 0; // Diz qual carta do baralho de 50 vamos comprar agora

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
    
    [Header("Placares Visuais")]
    public TextMeshProUGUI placarPontos;   // Pontos da rodada atual (até 3)
    public TextMeshProUGUI placarVitorias; // Rodadas vencidas no jogo (Melhor de 5)
    
    [Header("Estado do Jogo")]
    public bool turnoDoJogador = true; 
    public CardDisplay cartaDoJogadorNaArena; 
    public bool jogoPausado = false; 

    [Header("Visualizador de Cemitério")]
    public GameObject painelVisualizadorCemiterio;
    public Transform conteudoGradeCemiterio; // O "Content" do Scroll View
    public GameObject prefabIconeCemiterio;  // O botão miniatura que criamos no Passo 2

    // Variáveis de Placar Duplo
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

        vitoriasJogador = 0;
        vitoriasOponente = 0;
        pontosJogador = 0;
        pontosOponente = 0;
        indiceCompra = 0;
        AtualizarPlacares();

        EmbaralharBaralho();
        StartCoroutine(DistribuirCartasAnimado());
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
            if (indiceCompra >= baralhoCompleto.Count) break; // Segurança caso o baralho acabe

            // 1. Compra carta do Jogador
            GameObject novaCartaJogador = Instantiate(cartaPrefab, maoJogador);
            CardDisplay displayJogador = novaCartaJogador.GetComponent<CardDisplay>();
            displayJogador.cardData = baralhoCompleto[indiceCompra];
            displayJogador.pertenceAoJogador = true;
            displayJogador.AtualizarCarta();
            indiceCompra++; // Move o marcador do baralho

            if (indiceCompra >= baralhoCompleto.Count) break;

            // 2. Compra carta do Oponente
            GameObject novaCartaAdversario = Instantiate(cartaPrefab, maoAdversario);
            CardDisplay displayAdversario = novaCartaAdversario.GetComponent<CardDisplay>();
            displayAdversario.cardData = baralhoCompleto[indiceCompra]; 
            displayAdversario.pertenceAoJogador = false;
            displayAdversario.AtualizarCarta();
            indiceCompra++; // Move o marcador do baralho

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
        // 1. Removemos a trava do "jogoPausado", pois queremos inspecionar cartas a qualquer momento!
        if (painelCartaDetalhe == null) return;

        painelCartaDetalhe.gameObject.SetActive(true);
        
        // 2. Esta linha nova garante que a carta gigante pule para a FRENTE do cemitério
        painelCartaDetalhe.transform.SetAsLastSibling(); 
        
        painelCartaDetalhe.cardData = dados;
        painelCartaDetalhe.AtualizarCarta();
    }

    public void ReceberCartaNaArena(CardDisplay cartaDoJogador) 
    {
        if (jogoPausado) return;

        cartaDoJogadorNaArena = cartaDoJogador; 

        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);

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

        // Verifica se a rodada acabou (alguém fez 3 pontos ou a mão esvaziou)
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

    // NOVA COROUTINE: Reinicia a mesa para uma nova rodada
    private System.Collections.IEnumerator EncerrarRodada()
    {
        jogoPausado = true;
        
        string msgRodada = "";
        if (pontosJogador > pontosOponente) { vitoriasJogador++; msgRodada = "VOCÊ VENCEU A RODADA!"; turnoDoJogador = true; }
        else if (pontosOponente > pontosJogador) { vitoriasOponente++; msgRodada = "A IA VENCEU A RODADA!"; turnoDoJogador = false; }
        else { msgRodada = "RODADA EMPATADA!"; }
        
        AtualizarPlacares();

        if (textoResultado != null)
        {
            textoResultado.text = msgRodada;
            textoResultado.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);
        if (textoResultado != null) textoResultado.gameObject.SetActive(false);

        // Limpa as cartas que sobraram na mão
        LimparMaos();
        
        // Espera as cartas caírem no cemitério
        yield return new WaitForSeconds(1f); 

        // Checa se alguém já ganhou a Melhor de 5, ou se acabou o baralho inteiro
        if (vitoriasJogador >= 3 || vitoriasOponente >= 3 || indiceCompra >= baralhoCompleto.Count)
        {
            FinalizarJogo();
        }
        else
        {
            // Começa nova rodada
            pontosJogador = 0;
            pontosOponente = 0;
            AtualizarPlacares();
            StartCoroutine(DistribuirCartasAnimado());
        }
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
        
        string msgFinal = "";
        if (vitoriasJogador > vitoriasOponente) msgFinal = "VITÓRIA SUPREMA!\nVocê venceu o jogo!";
        else if (vitoriasOponente > vitoriasJogador) msgFinal = "GAME OVER\nA IA venceu o jogo!";
        else msgFinal = "EMPATE TÉCNICO!";

        if (textoResultado != null)
        {
            textoResultado.text = msgFinal;
            textoResultado.gameObject.SetActive(true);
            textoResultado.transform.SetAsLastSibling(); 
        }
        
        Debug.Log(msgFinal.Replace("\n", " "));
    }

    private void EnviarParaCemiterio(CardDisplay carta, Transform cemiterio)
    {
        if (carta == null || cemiterio == null) return;
        
        // CORREÇÃO DO GLITCH: Garante que a carta fique de frente no cemitério
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
        // Pausa o jogo para evitar que cartas sejam arrastadas no fundo
        jogoPausado = true;

        painelVisualizadorCemiterio.SetActive(true);
        painelVisualizadorCemiterio.transform.SetAsLastSibling(); 

        // 1. Limpa a lista antiga
        foreach (Transform filho in conteudoGradeCemiterio)
        {
            Destroy(filho.gameObject);
        }

        Transform cemiterioAlvo = ehCemiterioJogador ? cemiterioJogador : cemiterioOponente;

        // 2. Lê cada carta e cria uma cópia visual na lista
        foreach (Transform filho in cemiterioAlvo)
        {
            CardDisplay cartaNoCemiterio = filho.GetComponent<CardDisplay>();
            
            if (cartaNoCemiterio != null && cartaNoCemiterio.cardData != null)
            {
                // MUDANÇA PRINCIPAL: Instancia o PREFAB DA CARTA COMPLETA
                // (Você vai precisar arrastar seu prefab 'CartaVisual' para o slot no Inspector)
                GameObject novaCarta = Instantiate(prefabIconeCemiterio, conteudoGradeCemiterio);
                
                // Configura o CardDisplay da nova cópia
                CardDisplay displayNovaCarta = novaCarta.GetComponent<CardDisplay>();
                if (displayNovaCarta != null)
                {
                    displayNovaCarta.cardData = cartaNoCemiterio.cardData;
                    
                    // TRUQUE: Definimos 'pertenceAoJogador = true' para garantir que a carta
                    // apareça virada para cima e que o clique para inspecionar funcione.
                    // Como 'jogoPausado' está true, ela não poderá ser arrastada.
                    displayNovaCarta.pertenceAoJogador = true; 
                    
                    displayNovaCarta.AtualizarCarta();
                }
                // Não precisamos mais adicionar botão via código, o próprio CardDisplay já cuida do clique!
            }
        }
    }

    public void FecharCemiterio()
    {
        painelVisualizadorCemiterio.SetActive(false);
        // Despausa o jogo ao fechar o painel
        jogoPausado = false;
    }
    public void ClicarCemiterioJogador() { AbrirCemiterio(true); }
    public void ClicarCemiterioOponente() { AbrirCemiterio(false); }
}