using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // <-- NOVO: Necessário para trocar de cena!

public class GerenciadorDeBatalha : MonoBehaviour
{
    [Header("Configurações da Partida")]
    public BotProfile oponenteAtual; 

    [Header("Decks da Partida (Prontos para o combate)")]
    public List<CardData> deckDoJogador = new List<CardData>();
    public List<CardData> deckDoBot = new List<CardData>();

    // --- NOVO: REFERÊNCIA PARA A JANELA ---
    [Header("Interface (UI)")]
    public GameObject painelConfiguracoes;

    void Start()
    {
        // Garante que a janelinha comece invisível quando a luta iniciar
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);

        PrepararPartida();
    }

    private void PrepararPartida()
    {
        CarregarDeckDoJogadorSalvo();

        if (deckDoJogador.Count == 0)
        {
            Debug.LogError("❌ ERRO: O deck do jogador está vazio! Volte ao menu e salve um deck de 25 cartas.");
            return; 
        }

        if (oponenteAtual != null) deckDoBot = new List<CardData>(oponenteAtual.deckPreDefinido);
        else { Debug.LogError("⚠️ Faltou colocar o Oponente (BotProfile) no Inspector!"); return; }

        EmbaralharDeck(deckDoJogador);
        EmbaralharDeck(deckDoBot);

        Debug.Log($"⚔️ BATALHA INICIADA! Jogador ({deckDoJogador.Count} cartas) VS {oponenteAtual.nomeDoBot} ({deckDoBot.Count} cartas)");
    }

    private void CarregarDeckDoJogadorSalvo()
    {
        deckDoJogador.Clear();
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
                    if (cartaEncontrada != null) deckDoJogador.Add(cartaEncontrada);
                }
            }
        }
    }

    private void EmbaralharDeck(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // ==========================================
    // --- FUNÇÕES DOS BOTÕES E FIM DE JOGO ---
    // ==========================================

    public void AbrirConfiguracoes()
    {
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
    }

    public void RenderSe()
    {
        Debug.Log("🏳️ O jogador fugiu da batalha! Voltando ao Menu Principal...");
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void FinalizarBatalha()
    {
        // Esta função será chamada no futuro, quando as cartas de alguém acabarem!
        Debug.Log("🏆 Batalha encerrada! Retornando ao Menu Principal...");
        SceneManager.LoadScene("MenuPrincipal");
    }
}