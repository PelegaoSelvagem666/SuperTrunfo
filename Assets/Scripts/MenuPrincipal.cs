using UnityEngine;
using UnityEngine.SceneManagement; // <-- BIBLIOTECA OBRIGATÓRIA PARA TROCAR DE CENA!

public class MenuPrincipal : MonoBehaviour
{
    // O nome exato das suas cenas. Mude aqui se os nomes estiverem diferentes!
    public string nomeCenaDeckBuilder = "EditorDeck"; 
    public string nomeCenaBatalha = "CampoBatalha"; 

    public void IrParaDeckBuilder()
    {
        Debug.Log("Abrindo o Montador de Decks...");
        SceneManager.LoadScene(nomeCenaDeckBuilder);
    }

    public void IrParaBatalha()
    {
        Debug.Log("Entrando na Arena...");
        SceneManager.LoadScene(nomeCenaBatalha);
    }

    public void SairDoJogo()
    {
        Debug.Log("Fechando o jogo...");
        Application.Quit(); // Nota: Isso não fecha o editor da Unity, só funciona no jogo compilado!
    }
}