using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para trocar de cena

public class GerenciadorDeDeck : MonoBehaviour
{
    // O padrão Singleton (instancia) permite que qualquer script do jogo acesse essa "mochila"
    public static GerenciadorDeDeck instancia;

    [Header("O Deck Escolhido")]
    public List<CardData> deckAtivo = new List<CardData>();

    void Awake()
    {
        // Se a mochila não existe, eu sou a mochila e me torno imortal!
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Este é o feitiço de imortalidade entre cenas
        }
        else
        {
            // Se já existir uma mochila (ex: você voltou pro menu depois da batalha), destrua a cópia
            Destroy(gameObject); 
        }
    }

    // Função para chamar quando clicar no botão "JOGAR"
    public void IrParaBatalha(string nomeDaCenaDeBatalha)
    {
        if (deckAtivo.Count > 0) // Só deixa jogar se tiver carta
        {
            Debug.Log("Indo para a batalha com " + deckAtivo.Count + " cartas!");
            SceneManager.LoadScene(nomeDaCenaDeBatalha);
        }
        else
        {
            Debug.LogWarning("Seu deck está vazio! Adicione cartas antes de jogar.");
        }
    }
}