# Segmentos de `c_ClaveProdServ` (referencia para armar un manifiesto)

`ClaveProdServ` es un codigo de 8 digitos basado en el estandar **UNSPSC**; los primeros
2 digitos son el **segmento** (la categoria de negocio) — es lo que `claveProdServ.segmentos`
en el manifiesto de curacion filtra por prefijo (ver `../README.md`).

**Estos nombres NO vienen de `catCFDI.xsd`** — ese archivo no trae documentacion para
ninguno de los 5 catalogos grandes (ver `TRSF.Invoicing/Schemas40/xsd/README.md`). Vienen
del estandar UNSPSC publico en el que se basa el catalogo de SAT, mas confirmacion
puntual contra el catalogo real de SAT (que si trae descripciones por codigo, a
diferencia del XSD) para los casos donde habia duda. Esta tabla es para decidir **que
segmentos le interesan a un negocio** al armar un manifiesto — no sustituye verificar el
codigo exacto de 8 digitos de cada producto/servicio en el buscador oficial de SAT antes
de timbrar un CFDI real.

Solo se listan los 58 segmentos que **realmente existen** en el catalogo vendorizado
(`TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite`) — verificado por consulta directa,
con el numero real de codigos por segmento a la fecha de este catalogo.

| Segmento | Nombre | Codigos |
|---|---|---|
| `01` | Generico / no clasificado — en la practica es un solo codigo, `01010101` ("No existe en el catalogo"), el que SAT recomienda usar cuando ningun otro codigo aplica | 1 |
| `10` | Material vivo de origen vegetal y animal, y sus accesorios y suministros | 7,878 |
| `11` | Materiales minerales y textiles, y materiales vegetales y animales no comestibles | 395 |
| `12` | Quimicos, incluyendo bioquimicos, y materiales gaseosos | 471 |
| `13` | Resinas, cauchos, espumas, peliculas y materiales elastomericos | 192 |
| `14` | Materiales y productos de papel | 143 |
| `15` | Combustibles, aditivos de combustibles, lubricantes y materiales anticorrosivos | 102 |
| `20` | Maquinaria y accesorios para mineria y perforacion de pozos | 726 |
| `21` | Maquinaria y accesorios agricolas, pesqueros, forestales y de vida silvestre | 117 |
| `22` | Maquinaria y accesorios para construccion y edificacion | 106 |
| `23` | Maquinaria y accesorios para manufactura y procesamiento industrial | 834 |
| `24` | Maquinaria de manejo de materiales, acondicionamiento y almacenamiento, y sus accesorios | 370 |
| `25` | Vehiculos comerciales, militares y privados, y sus accesorios y componentes | 820 |
| `26` | Maquinaria de generacion y distribucion de energia, y sus accesorios | 556 |
| `27` | Herramientas y maquinaria general | 635 |
| `30` | Estructuras y componentes de construccion, edificacion y manufactura, y sus suministros | 970 |
| `31` | Componentes de manufactura y suministros | 2,964 |
| `32` | Componentes y suministros electronicos | 306 |
| `39` | Sistemas electricos y de iluminacion, y sus componentes, accesorios y suministros | 605 |
| `40` | Sistemas y equipo de distribucion y acondicionamiento, y sus componentes | 1,013 |
| `41` | Equipo de laboratorio, medicion, observacion y prueba | 1,876 |
| `42` | Equipo medico y sus accesorios y suministros | 2,838 |
| `43` | Tecnologias de informacion, radiodifusion y telecomunicaciones | 705 |
| `44` | Equipo, accesorios y suministros de oficina | 395 |
| `45` | Equipo y suministros de impresion, fotografia, audio y video | 266 |
| `46` | Equipo y suministros de defensa, seguridad publica y proteccion | 386 |
| `47` | Equipo y suministros de limpieza | 233 |
| `48` | Maquinaria, equipo y suministros de la industria de servicios | 192 |
| `49` | Equipo y accesorios deportivos y recreativos | 332 |
| `50` | Alimentos, bebidas y productos de tabaco | 17,974 |
| `51` | Medicamentos y productos farmaceuticos | 1,892 |
| `52` | Aparatos domesticos, suministros y productos electronicos de consumo | 351 |
| `53` | Ropa, equipaje y productos de cuidado personal | 294 |
| `54` | Relojes, joyeria y productos de piedras preciosas | 65 |
| `55` | Productos editoriales/publicados | 125 |
| `56` | Muebles y mobiliario | 266 |
| `60` | Instrumentos musicales, juegos, juguetes, artes y manualidades, y equipo educativo | 1,388 |
| `64` | Sin identificar con certeza — un solo codigo (`64122100`); no encontrado en las fuentes de referencia consultadas | 1 |
| `70` | Servicios agricolas, de pesca, forestales y de vida silvestre por contrato | 271 |
| `71` | Servicios de mineria, petroleo y gas | 535 |
| `72` | Servicios de construccion y mantenimiento de edificios e instalaciones | 472 |
| `73` | Servicios de produccion y manufactura industrial | 317 |
| `76` | Servicios de limpieza industrial | 89 |
| `77` | Servicios ambientales | 99 |
| `78` | Servicios de transporte, almacenamiento y correo | 142 |
| `80` | Servicios de gestion empresarial y profesionales administrativos | 226 |
| `81` | Servicios de ingenieria, investigacion y tecnologia | 263 |
| `82` | Servicios editoriales, de diseno, graficos y de artes | 194 |
| `83` | Servicios publicos y relacionados con el sector publico | 112 |
| `84` | Servicios financieros y de seguros | 110 |
| `85` | Servicios de salud | 189 |
| `86` | Servicios de educacion y capacitacion | 120 |
| `90` | Servicios de viajes, alimentacion, hospedaje y entretenimiento | 92 |
| `91` | Servicios personales y domesticos | 47 |
| `92` | Servicios de defensa nacional, orden publico y seguridad | 80 |
| `93` | Servicios politicos y de asuntos civiles | 252 |
| `94` | Organizaciones y clubes | 120 |
| `95` | Terrenos, edificios, estructuras y vias de comunicacion (incluye instalaciones prefabricadas/moviles) | 234 |

Ver `samples/*.json` para manifiestos de ejemplo que usan varios de estos segmentos por
industria.
