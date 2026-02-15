using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Configurações do Baralho")]
    public List<CardData> baralhoCompleto;
    public GameObject cartaPrefab;

    [Header("Áreas do Tabuleiro")]
    public Transform maoJogador;
    public Transform maoAdversario; // O novo espaço que criamos!

    void Start()
    {
        EmbaralharBaralho();
        ComprarCartasIniciais();
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

    public void ComprarCartasIniciais()
    {
        // 1. O Jogador compra as 4 primeiras cartas da lista misturada (0 a 3)
        for (int i = 0; i < 4; i++)
        {
            GameObject novaCarta = Instantiate(cartaPrefab, maoJogador);
            CardDisplay display = novaCarta.GetComponent<CardDisplay>();
            
            display.cardData = baralhoCompleto[i];
            display.AtualizarCarta(); 
        }

        // 2. O Adversário compra as 4 PRÓXIMAS cartas da lista (4 a 7)
        for (int i = 4; i < 8; i++)
        {
            GameObject novaCarta = Instantiate(cartaPrefab, maoAdversario);
            CardDisplay display = novaCarta.GetComponent<CardDisplay>();
            
            // Aqui passamos o [i] que vai de 4 até 7
            display.cardData = baralhoCompleto[i];
            display.AtualizarCarta(); 
        }
    }
}