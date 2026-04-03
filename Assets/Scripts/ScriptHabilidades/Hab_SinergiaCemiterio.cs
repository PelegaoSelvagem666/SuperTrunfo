using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaSinergia", menuName = "Habilidades/Sinergia de Cemitério")]
public class Hab_SinergiaCemiterio : HabilidadeBase
{
    [Header("Configuração da Sinergia")]
    [Tooltip("Qual é o nome exato da carta parceira que deve estar no cemitério?")]
    public string nomeCartaParceira;

    [Tooltip("Para qual valor os atributos devem subir?")]
    public int poderTotal = 1000;

    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay cartaUsuario)
    {
        // 1. Descobre se deve olhar o cemitério do Jogador ou do Bot
        Transform cemiterioAlvo = cartaUsuario.pertenceAoJogador ? 
            GameManager.instancia.cemiterioJogador : 
            GameManager.instancia.cemiterioOponente;

        bool parceiroEncontrado = false;

        // 2. Vasculha os mortos procurando o parceiro pelo NOME
        foreach (Transform filho in cemiterioAlvo)
        {
            CardDisplay cartaMorta = filho.GetComponent<CardDisplay>();
            if (cartaMorta != null && cartaMorta.cardData.nomeCarta == nomeCartaParceira)
            {
                parceiroEncontrado = true;
                break; // Achou! Não precisa continuar procurando
            }
        }

        // 3. Se o parceiro estiver morto, o combo é ativado!
        if (parceiroEncontrado)
        {
            // Pega o atributo que está sendo disputado nesta rodada
            string atributoAtual = GameManager.instancia.atributoEmDisputa;
            
            // Lê o valor base que a carta tem (Ex: 300 de Força)
            int valorBase = GameManager.instancia.PegarValorAtributo(cartaUsuario.cardData, atributoAtual);
            
            // Calcula a diferença para cravar em 1000 (Ex: 1000 - 300 = +700 de bônus)
            int bonusNecessario = poderTotal - valorBase;

            // Aplica o bônus na arena
            cartaUsuario.valorTemporarioBonus += bonusNecessario;
            cartaUsuario.AtualizarCarta();

            Debug.Log($"<color=magenta>[SINERGIA ATIVADA]</color> {cartaUsuario.cardData.nomeCarta} sentiu a queda de {nomeCartaParceira}! Poder elevado para {poderTotal}!");

            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = $"<size=45>Sinergia Ativada!\nO poder de {nomeCartaParceira} ecoa!</size>";
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(2.5f);
        }
    }
}