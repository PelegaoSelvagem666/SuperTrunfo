using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoForcarEmpate", menuName = "Habilidades/Forçar Empate (Soma Total)")]
public class Hab_ForcarEmpate : HabilidadeBase
{
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        // 1. Descobre qual atributo está sendo disputado nesta rodada
        string atributo = GameManager.instancia.atributoEmDisputa;

        // 2. Pega os valores BASE de cada carta
        int baseUsuario = GameManager.instancia.PegarValorAtributo(cartaUsuario.cardData, atributo);
        int baseInimigo = GameManager.instancia.PegarValorAtributo(cartaInimiga.cardData, atributo);

        // 3. Calcula o total atual (Base + Possíveis Buffs ou Nerfs que já tenham ocorrido)
        int totalUsuario = baseUsuario + cartaUsuario.valorTemporarioBonus;
        int totalInimigo = baseInimigo + cartaInimiga.valorTemporarioBonus;

        // 4. A MÁGICA: Se o usuário estiver perdendo, ele iguala!
        if (totalUsuario < totalInimigo)
        {
            // Calcula exatamente quantos pontos faltam para empatar
            int pontosNecessarios = totalInimigo - totalUsuario;

            Debug.Log($"[{cartaUsuario.cardData.nomeCarta}] ativou a habilidade! Adicionando +{pontosNecessarios} para forçar o empate.");

            // Injeta os pontos na carta
            cartaUsuario.valorTemporarioBonus += pontosNecessarios;
            
            // Atualiza o visual para o jogador ver o número subindo magicamente até igualar o do inimigo
            cartaUsuario.AtualizarCarta();

            // Mensagem dramática na tela
            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = "Atributos Igualados!\nA disputa vai para a Soma Total!";
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }

            // Uma pausa dramática de 2 segundos para o jogador ver o empate acontecendo
            yield return new WaitForSeconds(2f);
        }
        else
        {
            Debug.Log($"[{cartaUsuario.cardData.nomeCarta}] já está ganhando ou empatando. A habilidade guardou as forças.");
        }
    }
}