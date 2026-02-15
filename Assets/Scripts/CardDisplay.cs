using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Header("O Cérebro da Carta")]
    public CardData cardData;

    [Header("Elementos Visuais Fixos (Arraste da Hierarchy)")]
    public TextMeshProUGUI textoNome;
    public Image imagemArte;
    // Novos campos para Tipo e Moral
    public TextMeshProUGUI textoTipo;
    public Image imagemMoral;
    
    [Header("Atributos")]
    public TextMeshProUGUI textoForca;
    public TextMeshProUGUI textoMagia;
    public TextMeshProUGUI textoAgilidade;
    public TextMeshProUGUI textoInteligencia;

    [Header("Banco de Ícones de Moral (Arraste da pasta Artes)")]
    // Aqui guardaremos as 3 opções de imagem para o script escolher
    public Sprite iconeBom;
    public Sprite iconeNeutro;
    public Sprite iconeMal;

    void Start()
    {
        // Se já tiver uma carta plugada, atualiza ao iniciar
        if (cardData != null) AtualizarCarta();
    }

    public void AtualizarCarta()
    {
        if (cardData == null) return;

        // 1. Textos Básicos e Arte Principal
        textoNome.text = cardData.nomeCarta;
        imagemArte.sprite = cardData.arteCarta;
        textoTipo.text = cardData.tipo.ToString(); // Converte o Enum do tipo para texto

        // 2. Atributos Numéricos
        textoForca.text = cardData.forca.ToString();
        textoMagia.text = cardData.magia.ToString();
        textoAgilidade.text = cardData.agilidade.ToString();
        textoInteligencia.text = cardData.inteligencia.ToString();

        // 3. Lógica de Troca do Ícone de Moral (O "Switch")
        switch (cardData.moral)
        {
            case CardMoral.Bom:
                imagemMoral.sprite = iconeBom;
                break;
            case CardMoral.Neutro:
                imagemMoral.sprite = iconeNeutro;
                break;
            case CardMoral.Mal:
                imagemMoral.sprite = iconeMal;
                break;
        }
    }
}