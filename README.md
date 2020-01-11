=== cmdradio-visual

This is simple UI for cmdradio project (dead now https://habr.com/ru/post/196612/)

Shoutcast is dead too, now there is only icecast directory alive

Relevand links:

http://dir-test.xiph.org

https://people.xiph.org/~epirat/yp_apidoc/

http://dir.xiph.org

```
0 ➜ curl -i "http://dir-test.xiph.org/streams?genre=gold" -H "Accept: application/json"
HTTP/1.1 200 OK
Server: nginx/1.6.2
Date: Sat, 11 Jan 2020 15:03:14 GMT
Content-Type: application/json; charset=utf-8
Content-Length: 476
Connection: keep-alive
X-Powered-By: Express
ETag: W/"1dc-gLprPU7CQh/QwX1XB0if4S45rBY"

{"streams":[{"id":94501,"stream_name":"Zépices Radio","stream_type":"audio/mpeg","description":null,"songname":"COUPE CLOUE - Fam'm Kolokinte","url":"http://www.zepices-radio.com","avg_listening_time":null,"codec_sub_types":["MP3"],"bitrate":128,"hits":null,"cm":null,"samplerate":null,"channels":null,"quality":null,"genres":["gold","hits","local","folk","afric"],"listenurls":["http://www.radioking.com/play/epices-radio/51025"],"listeners":0,"max_listeners":0}],"data":{}}
```