using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoBot", menuName = "Super Trunfo/Oponente Bot")]
public class BotProfile : ScriptableObject
{
    [Header("Identidade do Bot")]
    public string nomeDoBot;
    public Sprite fotoDoBot; // Um avatar para ele
    
    [Range(1, 5)]
    [Tooltip("1 = Fácil, 5 = Impossível")]
    public int nivelDeDificuldade = 1;

    [Header("O Deck do Bot (EXATAS 25 CARTAS)")]
    public List<CardData> deckPreDefinido;
}