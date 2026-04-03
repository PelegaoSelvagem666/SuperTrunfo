using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuPrincipal : MonoBehaviour
{
    public string nomeCenaDeckBuilder = "EditorDeck"; 
    public string nomeCenaBatalha = "CampoBatalha"; 
    
    // Agora o seu botão do Menu "Montar Decks" tem que abrir a nova tela de seleção!
    public string nomeCenaSelecao = "SelecaoDeck"; 

  public void IrParaDeckBuilder()
    {
        Debug.Log("Abrindo a Seleção de Decks...");
        // Mudamos o destino para a sua nova cena!
        SceneManager.LoadScene("SelecaoDeck"); 
    }

    public void IrParaBatalha()
    {
        // 1. O jogo pergunta para a memória: "Qual é o deck favorito do jogador?"
        string deckPrincipal = PlayerPrefs.GetString("DeckPrincipalID", "");

        // Se o cara nunca escolheu um deck na vida, o botão bloqueia a ida pra Arena!
        if (string.IsNullOrEmpty(deckPrincipal))
        {
            Debug.LogWarning("Você ainda não escolheu um Deck Principal! Vá em 'Decks' e clique em 'Usar em Batalha' em um deles.");
            return; 
        }

        // 2. Coloca o ID do deck certo na mochila
        if (GerenciadorDeDeck.instancia != null)
            GerenciadorDeDeck.instancia.deckIdAtual = deckPrincipal;

        // 3. Abre a porrada!
        SceneManager.LoadScene(nomeCenaBatalha);
    }

    public void SairDoJogo() { Application.Quit(); }
}