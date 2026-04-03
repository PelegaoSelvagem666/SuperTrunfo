using System.Collections;
using UnityEngine;

// Cria um botão no menu de clique direito da Unity para você fabricar essa habilidade fácil!
[CreateAssetMenu(fileName = "NovaHabilidadeAnularBuffs", menuName = "Habilidades/Anular Buffs")]
public class Hab_AnularBuffs : HabilidadeBase 
{
    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay cartaUsuario)
    {
        // Opcional: Aqui você pode colocar uma animação ou som quando ela é jogada na mesa.
        Debug.Log($"{cartaUsuario.cardData.nomeCarta} entrou em campo com uma aura anuladora!");
        yield break;
    }

    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        Debug.Log($"Analisando os status de {cartaInimiga.cardData.nomeCarta}...");

        // A MÁGICA ESTÁ AQUI:
        // Se o valor temporário for positivo (Buff), nós destruímos ele.
        // Se for negativo (Nerf), a condição dá falso e ele continua enfraquecido!
        if (cartaInimiga.valorTemporarioBonus > 0)
        {
            Debug.Log($"Efeito Ativado! O buff de +{cartaInimiga.valorTemporarioBonus} do oponente foi dissipado!");
            
            // Zera completamente a vantagem do inimigo
            cartaInimiga.valorTemporarioBonus = 0;
            
            // Atualiza o visual da carta para o jogador ver o número do inimigo caindo na hora!
            cartaInimiga.AtualizarCarta();

            // Pequena pausa dramática na batalha para todo mundo ver o efeito acontecendo
            yield return new WaitForSeconds(1f);
        }
        else if (cartaInimiga.valorTemporarioBonus < 0)
        {
            Debug.Log("O oponente possui um Nerf. A habilidade ignorou e manteve a penalidade!");
        }
        else
        {
            Debug.Log("O oponente não tinha nenhum buff para ser roubado ou cancelado.");
        }
    }
}