using UnityEngine;

public class GerenciadorPainelAtributos : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject painelVisual; // Arraste o próprio painel aqui para ele poder se desligar
    public Transform maoDoJogador; // Arraste o objeto "Mão" (Hand) onde as cartas ficam guardadas

    // Guarda a referência da carta que acabou de ser jogada
    private GameObject cartaNaArena; 

    // Você vai chamar essa função a partir do seu script de Drag & Drop quando a carta cair na arena
    public void MostrarPainel(GameObject cartaJogada)
    {
        cartaNaArena = cartaJogada;
        painelVisual.SetActive(true);
    }

    // --- FUNÇÕES DOS BOTÕES DE ATRIBUTO ---
    // Vamos passar o nome do atributo direto pelo Inspector da Unity
    public void SelecionarAtributo(string atributoEscolhido)
    {
        Debug.Log("Atributo escolhido para a luta: " + atributoEscolhido);
        
        // AQUI ENTRA A LÓGICA DE COMBATE
        // Exemplo: SistemaDeBatalha.CompararAtributos(cartaNaArena, atributoEscolhido);

        FecharPainel();
    }

    // --- FUNÇÃO DO BOTÃO VOLTAR ---
    public void CancelarJogada()
    {
        Debug.Log("Jogada cancelada. Devolvendo a carta para a mão.");
        
        if (cartaNaArena != null)
        {
            // Devolve a carta para o objeto da mão. 
            // Se a sua "mão" usa um Horizontal Layout Group, isso já vai organizar a carta automaticamente no lugar certo.
            cartaNaArena.transform.SetParent(maoDoJogador, false);
        }

        FecharPainel();
    }

    private void FecharPainel()
    {
        painelVisual.SetActive(false);
        cartaNaArena = null; // Limpa a memória
    }
}