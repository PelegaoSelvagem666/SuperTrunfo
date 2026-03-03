using UnityEngine;

public class TesteMontadorDeck : MonoBehaviour
{
    [Header("Arraste algumas cartas (ScriptableObjects) aqui para testar")]
    public CardData cartaExemploA;
    public CardData cartaExemploB;

    public void AdicionarCartaA()
    {
        GerenciadorDeDeck.instancia.deckAtivo.Add(cartaExemploA);
        Debug.Log($"Adicionou {cartaExemploA.nomeCarta}! Total: {GerenciadorDeDeck.instancia.deckAtivo.Count}");
    }

    public void AdicionarCartaB()
    {
        GerenciadorDeDeck.instancia.deckAtivo.Add(cartaExemploB);
        Debug.Log($"Adicionou {cartaExemploB.nomeCarta}! Total: {GerenciadorDeDeck.instancia.deckAtivo.Count}");
    }

    public void ClicarEmJogar()
    {
        // SUBSTITUA "SampleScene" PELO NOME EXATO DA SUA CENA DE BATALHA!
        GerenciadorDeDeck.instancia.IrParaBatalha("CampoBatalha"); 
    }
}