# Prueba Carvajal
---

## Caracteristicas del proyecto

### Endpoints del api:

* Get ```/api/User``` devuelve todos los usuarios en DB
* Get ```/api/User/{id}``` devuelve el usuario con el id especificado
* Post ```/api/User``` crea un usuario en DB
* Put ```/api/User/{id}``` modifica el usuario con el id especificado
* Delete ```/api/User/{id}``` elimina al usuario con el id especificado
* Get ```/api/User/getDocTypes``` devuelve los tipos de documento en DB
* Get ```/api/User/login/{num_documento}``` devuelve el usuario con el numero de documento especificado

### Motor de base de datos:

* PostgreSQL en localhost
* ConexionString situada en appsetings del api