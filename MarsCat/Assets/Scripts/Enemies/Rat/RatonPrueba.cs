// Asistencia de IA: escrito con Claude Code (Anthropic).
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Prueba automática del ratón en Play Mode (Tools > Ratón > 2). Se crea solo durante la prueba.
/// Mueve a Nio con la entrada simulada de NioMovement (mismo código que el teclado) y mide lo que hacen el ratón,
/// la puerta y DoorSecurityController. Incluye una prueba de lógica de CapsuleDomeController con piezas temporales.
/// Escribe Logs/Raton_PruebaPlayMode.txt y sale de Play Mode.
/// </summary>
public class RatonPrueba : MonoBehaviour
{
    RatEnemyAI ia; RatEnemyCombat combate; RatEnemyAnimation anim; Salud saludRata; RatEnemyController ctrl; NavMeshAgent agente;
    NioMovement nio; Salud saludNio; NioRespuestaDanio respuesta; DoorSecurityController seg;
    readonly StringBuilder sb = new StringBuilder(), notas = new StringBuilder();
    int ok, total;
    readonly HashSet<string> estadosAnim = new HashSet<string>();
    bool flipSi, flipNo, registrar = true;
    float maxFueraZona;
    float tPuertaCerrada = -1f, tSeguro = -1f;
    bool seguroValor;
    readonly List<(float t, EstadoRaton e)> cambiosEstado = new List<(float t, EstadoRaton e)>();

    static string Ruta(string n) { string d = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs"); Directory.CreateDirectory(d); return Path.Combine(d, n); }
    void C(bool cond, string texto) { total++; if (cond) ok++; sb.AppendLine((cond ? "[OK]    " : "[FALLO] ") + texto); }
    static Vector3 Plano(Vector3 v) { v.y = 0f; return v; }
    IEnumerator Esperar(float s) { float t = 0f; while (t < s) { yield return null; t += Time.deltaTime; } }

    string EstadoAnim()
    {
        AnimatorStateInfo i = anim.Animator.GetCurrentAnimatorStateInfo(0);
        foreach (string e in new[] { "Idle", "Walk", "Attack", "Damage", "Death" }) if (i.IsName(e)) return e;
        return "?";
    }

    Vector2 EntradaHacia(Vector3 dir)
    {
        dir = Plano(dir).normalized;
        Vector3 f = Plano(nio.movementReference.forward).normalized, r = Plano(nio.movementReference.right).normalized;
        return new Vector2(Vector3.Dot(dir, r), Vector3.Dot(dir, f));
    }

    float FueraDeZona(Vector3 p)
    {
        Vector3 d = p - seg.centroZona;
        return Mathf.Max(0f, Mathf.Abs(d.x) - seg.tamanoZona.x * 0.5f, Mathf.Abs(d.z) - seg.tamanoZona.z * 0.5f);
    }

    void LateUpdate()
    {
        if (!registrar || anim == null) return;
        estadosAnim.Add(EstadoAnim());
        if (agente != null && agente.enabled && Plano(agente.velocity).magnitude > 0.2f) { if (anim.Sprite.flipX) flipSi = true; else flipNo = true; }
        if (ia != null && ia.enabled) maxFueraZona = Mathf.Max(maxFueraZona, FueraDeZona(ia.transform.position));
    }

    IEnumerator Start()
    {
        foreach (GameObject r in gameObject.scene.GetRootGameObjects())
        {
            if (ia == null) ia = r.GetComponentInChildren<RatEnemyAI>();
            if (nio == null) nio = r.GetComponentInChildren<NioMovement>();
        }
        sb.AppendLine("PRUEBA DEL RATÓN ENEMIGO EN PLAY MODE — " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | Unity " + Application.unityVersion);
        if (ia == null || nio == null) { sb.AppendLine("ERROR: no se encontró el ratón o Nio_Player."); Terminar(); yield break; }
        combate = ia.GetComponent<RatEnemyCombat>(); ctrl = ia.GetComponent<RatEnemyController>(); saludRata = ia.GetComponent<Salud>();
        agente = ia.GetComponent<NavMeshAgent>(); anim = ia.GetComponentInChildren<RatEnemyAnimation>();
        saludNio = nio.GetComponent<Salud>(); respuesta = nio.GetComponent<NioRespuestaDanio>(); seg = ia.seguridad;
        if (combate == null || anim == null || saludNio == null || seg == null) { sb.AppendLine("ERROR: faltan componentes (combate, animación, Salud de Nio o seguridad)."); Terminar(); yield break; }
        cambiosEstado.Add((Time.time, ia.Estado));
        ia.AlCambiarEstado += e => cambiosEstado.Add((Time.time, e));
        seg.AlCambiarSeguridad += v => { seguroValor = v; if (v && tSeguro < 0f) tSeguro = Time.time; };

        yield return Esperar(1.0f);
        nio.usarEntradaSimulada = true;
        nio.entradaSimulada = Vector2.zero;
        if (!nio.controlActivo) nio.PulsarAlternarControl();
        yield return Esperar(0.2f);
        saludNio.Restaurar();

        // ---------------- 1. Estado inicial
        sb.AppendLine();
        sb.AppendLine("== 1. Estado inicial: Nio fuera de la zona técnica ==");
        C(seg.JugadorSeguro && !seg.JugadorEnZona && seg.PuertasCerradas, "Nio fuera de la zona y las puertas TEC cerradas: DoorSecurityController lo marca a salvo");
        C(ia.Estado != EstadoRaton.Persecucion, "El ratón no persigue a un jugador a salvo (estado " + ia.Estado + ")");

        // ---------------- 2. Jugador dentro del rango
        sb.AppendLine();
        sb.AppendLine("== 2. Jugador dentro del rango ==");
        Vector3 rata = ia.transform.position;
        Vector3 haciaCentro = Plano(seg.centroZona - rata).normalized;
        float xPasillo = seg.centroZona.x + 0.4f; // lado de las puertas, lejos de la consola
        Vector3 puntoNio = Vector3.zero;
        bool hay = false;
        foreach (float d in new[] { 4f, 3.5f, 3f, 4.5f })
        {
            Vector3 c = rata + haciaCentro * d;
            c.x = xPasillo;
            if (NavMesh.SamplePosition(new Vector3(c.x, 0f, c.z), out NavMeshHit h, 1f, NavMesh.AllAreas)) { puntoNio = new Vector3(c.x, 0f, h.position.z); hay = true; break; }
        }
        if (!hay) { C(false, "No se encontró un punto libre para Nio dentro de la zona"); Terminar(); yield break; }
        nio.Teletransportar(puntoNio);
        float t0 = Time.time;
        float d0 = Plano(nio.transform.position - ia.transform.position).magnitude;
        while (ia.Estado != EstadoRaton.Persecucion && Time.time - t0 < 2f) yield return null;
        float tDet = Time.time - t0;
        C(ia.Estado == EstadoRaton.Persecucion && tDet < 1f, "Nio a " + d0.ToString("0.0") + " m (radio de detección " + ia.radioDeteccion + " m) y a la vista: el ratón lo detecta en " + tDet.ToString("0.00") + " s y empieza a perseguirlo");
        C(!seg.JugadorSeguro && seg.JugadorEnZona, "Con Nio dentro de la zona, DoorSecurityController indica que NO está a salvo");
        yield return Esperar(0.5f);
        float d1 = Plano(nio.transform.position - ia.transform.position).magnitude;
        C(d1 < d0 - 0.4f, "El ratón se acerca: distancia de " + d0.ToString("0.00") + " a " + d1.ToString("0.00") + " m en 0,5 s");

        // ---------------- 3. Jugador en movimiento
        sb.AppendLine();
        sb.AppendLine("== 3. Jugador en movimiento ==");
        int rutas0 = ia.RutasCalculadas;
        int frames = 0, alineados = 0;
        nio.entradaSimulada = EntradaHacia(haciaCentro);
        float tm = 0f;
        while (tm < 1.5f)
        {
            yield return null;
            tm += Time.deltaTime;
            Vector3 v = Plano(agente.velocity);
            if (v.magnitude > 0.3f && !combate.Atacando)
            {
                frames++;
                if (Vector3.Angle(v, Plano(nio.transform.position - ia.transform.position)) < 35f) alineados++;
            }
        }
        nio.entradaSimulada = Vector2.zero;
        int rutas = ia.RutasCalculadas - rutas0;
        float pct = frames > 0 ? 100f * alineados / frames : 0f;
        C(rutas >= 5 && pct >= 80f, "Mientras Nio se mueve 1,5 s, el ratón recalcula su ruta " + rutas + " veces y avanza hacia él en el " + pct.ToString("0") + " % de los frames");

        // ---------------- 4. Ataque y daño
        sb.AppendLine();
        sb.AppendLine("== 4. Ataque y daño a Nio ==");
        saludNio.Restaurar();
        int vida0 = saludNio.Vida;
        var golpes = new List<float>();
        System.Action<Salud, bool> alImpactar = (s, aplicado) => { if (aplicado) golpes.Add(Time.time); };
        combate.AlImpactar += alImpactar;
        float tA = 0f;
        Vector3 posGolpe = Vector3.zero;
        bool parpadeo = false, ataqueAnim = false;
        float empuje = 0f;
        while (golpes.Count == 0 && tA < 5f)
        {
            yield return null;
            tA += Time.deltaTime;
            if (EstadoAnim() == "Attack") ataqueAnim = true;
        }
        if (golpes.Count > 0)
        {
            posGolpe = nio.transform.position;
            float te = 0f;
            while (te < 0.25f) { yield return null; te += Time.deltaTime; if (respuesta != null && respuesta.Parpadeando) parpadeo = true; if (EstadoAnim() == "Attack") ataqueAnim = true; }
            empuje = Plano(nio.transform.position - posGolpe).magnitude;
        }
        C(golpes.Count > 0 && ataqueAnim, "Con Nio quieto a su alcance, el ratón ataca (animación Attack) y le hace daño a los " + tA.ToString("0.0") + " s");
        C(golpes.Count > 0 && saludNio.Vida == vida0 - combate.danio, "El daño pasa por el sistema de vida: Nio baja de " + vida0 + " a " + saludNio.Vida + " puntos");
        C(parpadeo && empuje > 0.05f, "Respuesta de Nio: parpadeo rojo y empuje de " + empuje.ToString("0.00") + " m alejándose del ratón");

        float tObs = 0f;
        var tiemposAtaque = new List<float>();
        float ultimo = combate.UltimoAtaque;
        int framesObs = 0;
        while (tObs < 3.5f)
        {
            yield return null;
            tObs += Time.deltaTime;
            framesObs++;
            if (!Mathf.Approximately(combate.UltimoAtaque, ultimo)) { ultimo = combate.UltimoAtaque; tiemposAtaque.Add(ultimo); }
        }
        float minIntervalo = tiemposAtaque.Count > 1 ? tiemposAtaque.Zip(tiemposAtaque.Skip(1), (a, b) => b - a).Min() : float.MaxValue;
        int impactos3s = golpes.Count(g => g > Time.time - 3.5f);
        C(tiemposAtaque.Count >= 2 && minIntervalo >= combate.cooldown - 0.02f && impactos3s <= Mathf.CeilToInt(3.5f / combate.cooldown) + 1,
          "Tiempo de espera: " + tiemposAtaque.Count + " ataques en 3,5 s, separados al menos " + (minIntervalo == float.MaxValue ? 0f : minIntervalo).ToString("0.00") + " s (cooldown " + combate.cooldown +
          " s); " + impactos3s + " impactos en " + framesObs + " frames (nunca daño continuo)");

        // Invulnerabilidad: ataques más rápidos que la invulnerabilidad de Nio.
        saludNio.Restaurar();
        float cdOriginal = combate.cooldown;
        combate.cooldown = 0.35f;
        int ignorados0 = saludNio.GolpesIgnorados;
        golpes.Clear();
        yield return Esperar(3f);
        combate.cooldown = cdOriginal;
        float minGolpes = golpes.Count > 1 ? golpes.Zip(golpes.Skip(1), (a, b) => b - a).Min() : float.MaxValue;
        int ignorados = saludNio.GolpesIgnorados - ignorados0;
        C(golpes.Count >= 2 && minGolpes >= saludNio.invulnerabilidad - 0.02f && ignorados > 0,
          "Invulnerabilidad respetada: con ataques cada 0,35 s, Nio solo recibe daño cada " + (minGolpes == float.MaxValue ? 0f : minGolpes).ToString("0.00") + " s (invulnerabilidad " +
          saludNio.invulnerabilidad + " s) y " + ignorados + " mordiscos no le hacen nada");
        combate.AlImpactar -= alImpactar;

        // ---------------- 5 y 6. Escape por cada puerta de la zona
        CabinaPuertaAutomatica primera = seg.puertas.OrderBy(p => Vector3.Distance(p.transform.position, nio.transform.position)).First();
        CabinaPuertaAutomatica segunda = seg.puertas.First(p => p != primera);
        yield return EscapePor(primera, "5", false);
        yield return EscapePor(segunda, "6", true);
        C(maxFueraZona <= 0.05f, "El ratón no ha salido de la zona técnica en ninguna de las dos huidas (máximo fuera: " + maxFueraZona.ToString("0.00") + " m)");

        // ---------------- 7. Daño y muerte del ratón
        sb.AppendLine();
        sb.AppendLine("== 7. Daño y muerte del ratón ==");
        bool vioDamage = false;
        saludRata.RecibirDanio(1, null);
        float td = 0f;
        bool aturdido = ia.Estado == EstadoRaton.Aturdido;
        while (td < 0.4f) { yield return null; td += Time.deltaTime; if (EstadoAnim() == "Damage") vioDamage = true; }
        C(vioDamage && aturdido && saludRata.Vida == saludRata.vidaMaxima - 1, "Daño al ratón: animación Damage, se queda aturdido y baja a " + saludRata.Vida + " puntos");
        yield return Esperar(0.6f);
        saludRata.Matar(null);
        float tm2 = 0f;
        bool vioDeath = false;
        while (tm2 < 2.4f) { yield return null; tm2 += Time.deltaTime; if (EstadoAnim() == "Death") vioDeath = true; }
        Collider cuerpo = ia.GetComponent<Collider>();
        C(vioDeath && !ia.enabled && !agente.enabled && (cuerpo == null || !cuerpo.enabled) && anim.Sprite.color.a < 0.05f,
          "Muerte: animación Death, IA y movimiento detenidos, collider desactivado y sprite desvanecido (alfa " + anim.Sprite.color.a.ToString("0.00") + ")");
        registrar = false;

        sb.AppendLine();
        sb.AppendLine("== Animaciones y orientación ==");
        C(new[] { "Idle", "Walk", "Attack", "Damage", "Death" }.All(estadosAnim.Contains), "Estados del Animator vistos durante la prueba: " + string.Join(", ", estadosAnim.Where(e => e != "?")));
        C(flipSi && flipNo, "El ratón mira a izquierda y derecha invirtiendo el mismo sprite (flipX)");

        // ---------------- Domo: lógica de CapsuleDomeController con piezas temporales
        sb.AppendLine();
        sb.AppendLine("== CapsuleDomeController (prueba de lógica con piezas temporales, no es Capsula.blend) ==");
        yield return ProbarDomo();

        Terminar();
    }

    IEnumerator EscapePor(CabinaPuertaAutomatica puerta, string num, bool teletransportar)
    {
        sb.AppendLine();
        sb.AppendLine("== " + num + ". Nio escapa por la puerta " + puerta.puertaId + " y la puerta se cierra ==");
        saludNio.Restaurar();
        Bounds bp = puerta.colliderBloqueo.bounds;
        Vector3 normal = bp.size.x < bp.size.z ? Vector3.right : Vector3.forward;
        Vector3 fuera = Mathf.Sign(Vector3.Dot(bp.center - seg.centroZona, normal)) * normal;
        Vector3 centroPuerta = new Vector3(bp.center.x, 0f, bp.center.z);
        Vector3 dentro = centroPuerta - fuera * 1.0f;

        if (teletransportar)
        {
            // Nio vuelve a entrar en la zona entre el ratón y esta puerta, para que la huida sea con persecución.
            Vector3 rata = ia.transform.position;
            Vector3 dir = Plano(dentro - rata).normalized;
            float dist = Plano(dentro - rata).magnitude;
            Vector3 p = rata + dir * Mathf.Clamp(dist - 1.5f, 1.2f, 3.5f);
            p.x = seg.centroZona.x + 0.4f;
            p.y = 0f;
            nio.Teletransportar(p);
            float tp = 0f;
            while (ia.Estado != EstadoRaton.Persecucion && tp < 2f) { yield return null; tp += Time.deltaTime; }
        }
        float ySuelo = nio.transform.position.y;
        float dExt = DistanciaExterior(centroPuerta, fuera, ySuelo, out string diagSuelo);
        Vector3 exterior = centroPuerta + fuera * dExt;
        sb.AppendLine("        Suelo al otro lado de " + puerta.puertaId + " (y≈" + ySuelo.ToString("0.00") + "): " + diagSuelo);
        bool perseguia = ia.Estado == EstadoRaton.Persecucion;
        C(perseguia && !seg.JugadorSeguro, "Antes de huir, Nio está dentro de la zona y el ratón lo persigue (estado " + ia.Estado + ")");

        tPuertaCerrada = -1f;
        tSeguro = -1f;
        seguroValor = false;
        cambiosEstado.Clear();
        cambiosEstado.Add((Time.time, ia.Estado));
        int aperturasAntes = puerta.Aperturas;
        System.Action<CabinaPuertaAutomatica> alCerrar = q => { if (tPuertaCerrada < 0f && !seg.Contiene(nio.transform.position)) tPuertaCerrada = Time.time; };
        puerta.AlCerrarse += alCerrar;
        nio.correrSimulado = true;
        float tc = 0f;
        while (Plano(dentro - nio.transform.position).magnitude > 0.3f && tc < 10f) { nio.entradaSimulada = EntradaHacia(dentro - nio.transform.position); yield return null; tc += Time.deltaTime; }
        tc = 0f;
        while (Plano(exterior - nio.transform.position).magnitude > 0.3f && tc < 6f) { nio.entradaSimulada = EntradaHacia(exterior - nio.transform.position); yield return null; tc += Time.deltaTime; }
        nio.entradaSimulada = Vector2.zero;
        nio.correrSimulado = false;
        float tFuera = Time.time;
        bool nioFuera = !seg.Contiene(nio.transform.position);
        float tw = 0f;
        while (tSeguro < 0f && tw < 6f) { yield return null; tw += Time.deltaTime; }
        yield return null;
        CharacterController ccNio = nio.GetComponent<CharacterController>();
        float yFuera = nio.transform.position.y;
        C(nioFuera && puerta.Aperturas > aperturasAntes && dExt > 0f, "Nio atraviesa " + puerta.puertaId + " (la puerta se abre para él) y sale de la zona hasta un punto con suelo a " + dExt.ToString("0.0") + " m de la puerta");
        C(Mathf.Abs(yFuera - ySuelo) < 0.3f && (ccNio == null || ccNio.isGrounded), "Al otro lado Nio sigue de pie sobre el suelo (altura " + yFuera.ToString("0.00") + " m, no cae)");
        C(tPuertaCerrada > 0f, "La puerta se cierra sola detrás de Nio y lanza AlCerrarse (" + (tPuertaCerrada > 0f ? (tPuertaCerrada - tFuera).ToString("0.00") + " s después de salir" : "no llegó") + ")");
        C(tSeguro > 0f && Mathf.Abs(tSeguro - tPuertaCerrada) < 0.0001f && seguroValor,
          "DoorSecurityController marca a Nio a salvo en el mismo frame en que " + puerta.puertaId + " termina de cerrarse (por el estado de la puerta, no por distancia)");
        EstadoRaton antes = cambiosEstado.Where(x => x.t < tSeguro - 1e-5f).Select(x => x.e).DefaultIfEmpty(EstadoRaton.Patrulla).Last();
        bool regresoEnElFrame = cambiosEstado.Any(x => x.e == EstadoRaton.Regreso && Mathf.Abs(x.t - tSeguro) < 0.0001f);
        bool persiguiendo = antes == EstadoRaton.Persecucion || antes == EstadoRaton.Busqueda || antes == EstadoRaton.Aturdido;
        C(tSeguro > 0f && persiguiendo && regresoEnElFrame,
          "El ratón, que seguía a Nio (" + antes + "), abandona la persecución en ese mismo frame → " + (regresoEnElFrame ? "Regreso" : ia.Estado.ToString()));

        Vector3 rataAlCierre = ia.transform.position;
        int aperturasTrasCierre = puerta.Aperturas;
        int ataquesTrasCierre = combate.AtaquesIniciados;
        int vidaTrasCierre = saludNio.Vida;
        float dPuerta0 = Plano(centroPuerta - rataAlCierre).magnitude;
        yield return Esperar(3f);
        float dPuerta1 = Plano(centroPuerta - ia.transform.position).magnitude;
        float dCasa0 = Plano(ia.PosicionInicial - rataAlCierre).magnitude, dCasa1 = Plano(ia.PosicionInicial - ia.transform.position).magnitude;
        C(puerta.Cerrada && puerta.Aperturas == aperturasTrasCierre && (dPuerta1 > dPuerta0 + 0.3f || dCasa1 < 0.5f),
          "El ratón no intenta atravesar " + puerta.puertaId + ": sigue cerrada y él se aleja de ella (" + dPuerta0.ToString("0.0") + " → " + dPuerta1.ToString("0.0") +
          " m) hacia su posición inicial (" + dCasa0.ToString("0.0") + " → " + dCasa1.ToString("0.0") + " m)");
        float dNio = Vector3.Dot(Plano(nio.transform.position) - centroPuerta, fuera);
        C(combate.AtaquesIniciados == ataquesTrasCierre && saludNio.Vida == vidaTrasCierre && seg.JugadorSeguro && (!ia.LineaDeVision() || ia.Estado != EstadoRaton.Persecucion),
          "Con Nio fuera de la zona, al otro lado de la puerta cerrada (a " + dNio.ToString("0.0") + " m): 3 s sin ataques ni daño y sin persecución a través de la puerta");
        float tCasa = 0f;
        while (Plano(ia.PosicionInicial - ia.transform.position).magnitude > 0.35f && tCasa < 20f) { yield return null; tCasa += Time.deltaTime; }
        yield return Esperar(0.3f);
        C(Plano(ia.PosicionInicial - ia.transform.position).magnitude <= 0.35f && EstadoAnim() == "Idle", "El ratón vuelve a su posición inicial y se queda en Idle");
        C(Mathf.Abs(nio.transform.position.y - ySuelo) < 0.3f, "Nio sigue al otro lado, sobre el suelo, mientras el ratón vuelve (altura " + nio.transform.position.y.ToString("0.00") + " m)");
        puerta.AlCerrarse -= alCerrar;
    }

    // Busca el punto más alejado (hasta 3 m) al otro lado de la puerta donde hay suelo al mismo nivel y sitio libre para Nio.
    float DistanciaExterior(Vector3 centroPuerta, Vector3 fuera, float ySuelo, out string diag)
    {
        var partes = new List<string>();
        CharacterController cc = nio.GetComponent<CharacterController>();
        float elegido = 0f;
        foreach (float d in new[] { 3.0f, 2.5f, 2.0f, 1.6f, 1.3f })
        {
            Vector3 p = centroPuerta + fuera * d;
            Vector3 origen = new Vector3(p.x, ySuelo + 1.2f, p.z);
            RaycastHit[] hits = Physics.RaycastAll(origen, Vector3.down, 6f, ~0, QueryTriggerInteraction.Ignore)
                .Where(h => !h.collider.transform.IsChildOf(nio.transform) && !h.collider.transform.IsChildOf(ia.transform)).OrderBy(h => h.distance).ToArray();
            if (hits.Length == 0) { partes.Add(d.ToString("0.0") + " m: sin suelo"); continue; }
            RaycastHit h0 = hits[0];
            bool nivel = Mathf.Abs(h0.point.y - ySuelo) < 0.2f && h0.normal.y > 0.7f;
            float r = cc != null ? cc.radius : 0.22f, alto = cc != null ? cc.height : 0.62f;
            Vector3 pie = new Vector3(p.x, h0.point.y, p.z);
            bool libre = !Physics.CheckCapsule(pie + Vector3.up * (r + 0.05f), pie + Vector3.up * (alto - r), r * 0.9f, ~0, QueryTriggerInteraction.Ignore);
            partes.Add(d.ToString("0.0") + " m: " + h0.collider.name + " a y=" + h0.point.y.ToString("0.00") + (nivel ? "" : " (otro nivel)") + (libre ? "" : " (ocupado)"));
            if (nivel && libre) { elegido = d; break; }
        }
        diag = string.Join("; ", partes) + (elegido > 0f ? " → se usa " + elegido.ToString("0.0") + " m" : " → no hay punto válido");
        return elegido;
    }

    IEnumerator ProbarDomo()
    {
        var raiz = new GameObject("PruebaDomo_Temporal");
        raiz.SetActive(false);
        raiz.transform.position = new Vector3(60f, -40f, 60f);
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "Base";
        baseGo.transform.SetParent(raiz.transform, false);
        var bisagra = new GameObject("Bisagra_Domo").transform;
        bisagra.SetParent(raiz.transform, false);
        bisagra.localPosition = new Vector3(0f, 0.5f, -0.5f);
        var domo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        domo.name = "Domo";
        domo.transform.SetParent(bisagra, false);
        domo.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        var cdc = raiz.AddComponent<CapsuleDomeController>();
        cdc.piezas = new[] { new CapsuleDomeController.Pieza { transform = bisagra, rotacionAbierta = new Vector3(-100f, 0f, 0f) } };
        cdc.duracion = 1.6f;
        raiz.SetActive(true);
        yield return null;
        Quaternion cerrada = bisagra.localRotation;
        Vector3 basePos = baseGo.transform.position;
        Quaternion baseRot = baseGo.transform.rotation;
        yield return Esperar(0.5f);
        C(cdc.Cerrado && Quaternion.Angle(bisagra.localRotation, cerrada) < 0.01f, "Cerrada: el domo permanece en su posición inicial");
        cdc.Abrir();
        var angulos = new List<float>();
        for (int k = 0; k < 3; k++) { yield return Esperar(0.4f); angulos.Add(Quaternion.Angle(bisagra.localRotation, cerrada)); }
        while (cdc.EnMovimiento) yield return null;
        float final = Quaternion.Angle(bisagra.localRotation, cerrada);
        bool progresivo = angulos[0] > 1f && angulos[0] < angulos[1] && angulos[1] < angulos[2] && angulos[2] < 99f;
        C(progresivo, "Apertura progresiva: " + string.Join("° → ", angulos.Select(a => a.ToString("0"))) + "° a los 0,4 / 0,8 / 1,2 s");
        C(cdc.Abierto && Mathf.Abs(final - 100f) < 0.5f && baseGo.transform.position == basePos && baseGo.transform.rotation == baseRot,
          "Al terminar: domo abierto " + final.ToString("0.0") + "° y la base sin moverse");
        cdc.Cerrar();
        while (cdc.EnMovimiento) yield return null;
        C(Quaternion.Angle(bisagra.localRotation, cerrada) < 0.5f, "Cerrar() lo devuelve a la posición cerrada");
        Destroy(raiz);
    }

    void Terminar()
    {
        if (nio != null) { nio.usarEntradaSimulada = false; nio.entradaSimulada = Vector2.zero; nio.correrSimulado = false; }
        sb.AppendLine();
        if (notas.Length > 0) { sb.AppendLine("== NOTAS =="); sb.Append(notas); }
        sb.AppendLine("RESULTADO: " + ok + "/" + total + " comprobaciones superadas");
        File.WriteAllText(Ruta("Raton_PruebaPlayMode.txt"), sb.ToString());
        if (ok == total) Debug.Log("[Ratón] Prueba en Play Mode: " + ok + "/" + total + " OK. Informe: Logs/Raton_PruebaPlayMode.txt");
        else Debug.LogWarning("[Ratón] Prueba en Play Mode: " + ok + "/" + total + " OK. Revisa Logs/Raton_PruebaPlayMode.txt");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
