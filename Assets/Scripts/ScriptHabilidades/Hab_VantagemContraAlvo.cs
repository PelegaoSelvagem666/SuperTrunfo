using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoDeEfeitoAlvo { BuffarUsuario, DebuffarInimigo }
public enum LogicaDeAtivacao { OU, E }

[CreateAssetMenu(fileName = "Hab_VantagemContraAlvo", menuName = "Habilidades/Vantagem Contra Alvo")]
public class Hab_VantagemContraAlvo : HabilidadeBase
{
    [Header("Regra das Caixinhas")]
    public LogicaDeAtivacao regraDeAtivacao = LogicaDeAtivacao.OU;

    [Header("Condição de Ativação (Tipos)")]
    public bool checarTipo = true;
    public List<CardType> tiposAlvo;

    [Header("Condição de Ativação (Morais)")]
    public bool checarMoral = false;
    public List<CardMoral> moraisAlvo;

    [Header("Condição de Ativação (Classes)")]
    public bool checarClasse = false;
    public List<CardClass> classesAlvo;

    [Header("Efeito na Batalha")]
    public TipoDeEfeitoAlvo quemRecebeOEfeito;
    public int valorModificador = 50; 
    
    [TextArea(2, 3)]
    public string mensagemDeAtivacao = "Vantagem detectada!";

    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        bool passouNoTipo = false;
        bool passouNaMoral = false;
        bool passouNaClasse = false;

        if (checarTipo && tiposAlvo.Contains(cartaInimiga.cardData.tipo)) passouNoTipo = true;
        if (checarMoral && moraisAlvo.Contains(cartaInimiga.cardData.moral)) passouNaMoral = true;
        if (checarClasse && classesAlvo.Contains(cartaInimiga.cardData.classe)) passouNaClasse = true;

        bool alvoEncontrado = false;

        if (regraDeAtivacao == LogicaDeAtivacao.OU)
        {
            if ((checarTipo && passouNoTipo) || (checarMoral && passouNaMoral) || (checarClasse && passouNaClasse)) alvoEncontrado = true;
        }
        else if (regraDeAtivacao == LogicaDeAtivacao.E)
        {
            bool tipoOk = !checarTipo || passouNoTipo;
            bool moralOk = !checarMoral || passouNaMoral;
            bool classeOk = !checarClasse || passouNaClasse;
            // Só ativa se todos os que estiverem marcados forem verdadeiros
            if (tipoOk && moralOk && classeOk && (checarTipo || checarMoral || checarClasse)) alvoEncontrado = true;
        }

        if (alvoEncontrado)
        {
            if (quemRecebeOEfeito == TipoDeEfeitoAlvo.BuffarUsuario)
                cartaUsuario.valorTemporarioBonus += valorModificador;
            else if (quemRecebeOEfeito == TipoDeEfeitoAlvo.DebuffarInimigo)
                cartaInimiga.valorTemporarioBonus -= valorModificador;

            cartaUsuario.AtualizarCarta();
            cartaInimiga.AtualizarCarta();

            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = mensagemDeAtivacao;
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(2f);
        }
    }
}