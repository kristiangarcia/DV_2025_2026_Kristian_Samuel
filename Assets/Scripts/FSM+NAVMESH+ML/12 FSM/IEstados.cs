
using UnityEngine;

namespace DAM_IA_ML{ //Para que no choque con otro Script llamado igual

public interface IEstados {
    void Enter(Enemigo e);
    void Update(Enemigo e);
    void Exit(Enemigo e);
}

}