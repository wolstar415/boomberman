
const express = require('express');
var mysql = require('mysql');
const dbconfig = require('./config/config.js');
const sql = mysql.createConnection(dbconfig);
const bcrypt = require("bcrypt");
const app = express();
const http = require('http');
const server = http.createServer(app);
const port = 7777;
const { Server } = require("socket.io");
const e = require('express');
const io = new Server(server);





io.use((socket, next) => {
  if (socket.handshake.query.token === "UNITY" && socket.handshake.query.name === "boomberman" && socket.handshake.query.version === "0.1") {
    next();
  } else {
    next(new Error("인증 오류 "));
  }
});
var Users = [];
var Rooms = [];



io.on('connection', socket => {
  Users[socket.id] = {
    id: socket.id,
    loginID: "",
    Room: "",
    victory: 0,
    defeat: 0,
    draw: 0,
  }




  function RoomLobyInfoEmit() {

    var roomcheck = [];
    var i = 0;
    for (room in Rooms) {
      roomcheck.push({
        name: Rooms[room].name,
        currentP: Rooms[room].currentP,
        maxP: Rooms[room].maxP,
        seatInfos: Rooms[room].seatInfos,
        isPlaying: Rooms[room].isPlaying,
        mapIdx: Rooms[room].mapIdx
      })
      i++;
    }
    if (i == 0) {
      roomcheck = null;
    }
    io.to('Loby').emit('RoomReset', roomcheck)
  }


  socket.on('LoginCheck', (id, password) => {
    sql.query('SELECT * FROM users WHERE id=?', [id], function (error, results) {
      if (error) throw error;
      if (results.length > 0) {

        if (bcrypt.compareSync(password, results[0].password)) {
          //암호화된 비밀번호를 비교합니다.

          var check = true;
          for (var k in Users) {
            if (Users[k].loginID == id) {
              check = false;
              break;
            }
          }

          //현재 접속된 아이디가 있는지 파악합니다.
          if (check) {
            console.log(`${id} : 로그인 성공`)
            socket.join('Loby')
            Users[socket.id].loginID = id;
            Users[socket.id].victory = results[0].victory
            Users[socket.id].defeat = results[0].defeat
            Users[socket.id].draw = results[0].draw

            var roomcheck = [];
            var i = 0;
            for (room in Rooms) {
              roomcheck.push({
                name: Rooms[room].name,
                currentP: Rooms[room].currentP,
                maxP: Rooms[room].maxP,
                seatInfos: Rooms[room].seatInfos,
                isPlaying: Rooms[room].isPlaying,
                mapIdx: Rooms[room].mapIdx
              })
              i++;
            }
            if (i == 0) {
              roomcheck = null;
            }
            socket.emit('Login', results[0].victory, results[0].defeat, results[0].draw, roomcheck)
            sql.query('UPDATE users SET loginTime=? WHERE id=? ', [new Date(), id], function (error, results) {
              {
                if (error)
                  console.log(error)
              }
            })

          }
          else {
            //동일한 아이디가 들어왔으니 오류
            console.log("중복 불가!")
            socket.emit('Warnning', '이미 접속한 아이디입니다.')
          }


        }
        else {
          //비밀번호가 틀리면 오류
          console.log("비밀번호 틀림")
          socket.emit('Warnning', '비밀번호가 틀립니다.')
        }



      } else {
        //해당 아이디가 없으면 오류
        console.log("로그인 실패")
        socket.emit('Warnning', '아이디가 존재하지 않습니다.')
      }
    })
  })
  socket.on('CreateCheck', (id, password) => {
    //회원가입 체크

    sql.query('SELECT * FROM users WHERE id=?', [id], function (error, results) {
      //회원가입 하기전에 아이디 중복확인을 합니다.
      if (error) throw error;
      if (results.length > 0) {
        //아이디가 이미 있다면 실패
        console.log("회원가입 실패")
        socket.emit('Warnning', '이미 아이디가 있습니다.')
      }
      else {
        sql.query('INSERT INTO users (id, password, createTime) VALUES(?,?,?)', [id, bcrypt.hashSync(password, 10), new Date()], function (error, results) {
          if (error) {
            console.log(error)
          }
          else {
            console.log(`${id} : 회원가입 성공`)
            socket.emit('Create')
          }
        })

      }
    })
  })




  socket.on('JoinRoomCheck', (roomname, id) => {

    if (roomname in Rooms && Rooms[roomname].currentP < Rooms[roomname].maxP) {

      socket.leave('Loby');
      socket.join(roomname)
      Users[socket.id].Room = roomname;

      for (var index = 0; index < 4; index++) {
        if (Rooms[roomname].seatInfos[index].seatname == "") {
          Rooms[roomname].seatInfos[index].seatname = id
          Rooms[roomname].seatInfos[index].characterIdx = 0
          Rooms[roomname].seatInfos[index].Idx = 0
          break;
        }

      }

      Rooms[roomname].currentP++;

      var roomcheck = {
        name: Rooms[roomname].name,
        currentP: Rooms[roomname].currentP,
        maxP: Rooms[roomname].maxP,
        seatInfos: Rooms[roomname].seatInfos,
        isPlaying: Rooms[roomname].isPlaying,
        mapIdx: Rooms[roomname].mapIdx

      }
      const age = 3
      console.log(`${id} : 방참가 성공`)
      socket.emit('Join', roomname, roomcheck)
      socket.to(roomname).emit('SlotReset', roomcheck)
      RoomLobyInfoEmit()
    }
    else {
      console.log(`${id} : 방참가 실패`)
      socket.emit('Warnning', '방 참가 실패')
    }
  })


  socket.on('SlotOpen', (roomname, idx) => {


    Rooms[roomname].seatInfos[idx].seatname = "";
    Rooms[roomname].seatInfos[idx].characterIdx = 0;
    Rooms[roomname].seatInfos[idx].Idx = 0;
    Rooms[roomname].maxP++;

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    io.to(roomname).emit('SlotReset', roomcheck)
    console.log(`${roomname} : 슬룻 열음`)

    RoomLobyInfoEmit()
  })
  socket.on('SlotClose', (roomname, idx) => {


    Rooms[roomname].seatInfos[idx].seatname = "막음";
    Rooms[roomname].seatInfos[idx].characterIdx = 0;
    Rooms[roomname].seatInfos[idx].Idx = 0;
    Rooms[roomname].maxP--;

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    io.to(roomname).emit('SlotReset', roomcheck)
    console.log(`${roomname} : 슬룻 닫음`)
    RoomLobyInfoEmit()
  })
  socket.on('SlotMove', (roomname, check, idx) => {


    Rooms[roomname].seatInfos[idx].seatname = Rooms[roomname].seatInfos[check].seatname;
    Rooms[roomname].seatInfos[idx].characterIdx = Rooms[roomname].seatInfos[check].characterIdx;
    Rooms[roomname].seatInfos[idx].Idx = Rooms[roomname].seatInfos[check].Idx;

    Rooms[roomname].seatInfos[check].seatname = ""
    Rooms[roomname].seatInfos[check].characterIdx = 0
    Rooms[roomname].seatInfos[check].Idx = 0

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    io.to(roomname).emit('SlotReset', roomcheck)
    console.log(`${roomname} : 슬룻 이동`)
  })
  socket.on('SlotKick', (roomname, id, idx) => {


    socket.to(roomname).emit('KickCheck', id)

    Rooms[roomname].seatInfos[idx].seatname = "";
    Rooms[roomname].seatInfos[idx].characterIdx = 0;
    Rooms[roomname].seatInfos[idx].Idx = 0;
    Rooms[roomname].currentP--;

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    io.to(roomname).emit('SlotReset', roomcheck)
    console.log(`${roomname} : 강퇴 성공`)
    RoomLobyInfoEmit()
  })
  socket.on('KickCheck', (roomname) => {
    socket.leave(roomname);
    socket.join('Loby');
    Users[socket.id].Room = "";

    var roomcheck = [];
    var i = 0;
    for (room in Rooms) {
      roomcheck.push({
        name: Rooms[room].name,
        currentP: Rooms[room].currentP,
        maxP: Rooms[room].maxP,
        seatInfos: Rooms[room].seatInfos,
        isPlaying: Rooms[room].isPlaying,
        mapIdx: Rooms[room].mapIdx
      })
      i++;
    }
    if (i == 0) {
      roomcheck = null;
    }
    socket.emit('RoomReset', roomcheck)

  })

  socket.on('CharacterChange', (roomname, idx, characterIdx) => {



    Rooms[roomname].seatInfos[idx].characterIdx = characterIdx;

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    socket.to(roomname).emit('SlotReset', roomcheck)
    console.log(`${roomname} : 캐릭 변경`)
  })

  socket.on('MapSetting', (roomname, idx) => {



    Rooms[roomname].mapIdx = idx;

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx

    }
    console.log(`${roomname} : 맵 변경`)
    socket.to(roomname).emit('SlotReset', roomcheck)
  })

  socket.on('RoomReady', (roomname, idx) => {


    if (Rooms[roomname].seatInfos[idx].Idx == 0) {
      Rooms[roomname].seatInfos[idx].Idx = 1;
    }
    else {
      Rooms[roomname].seatInfos[idx].Idx = 0;
    }

    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx
    }
    console.log(`${roomname} : 준비 버튼 누름`)
    io.to(roomname).emit('SlotReset', roomcheck)
  })

  socket.on('GameStart', (roomname, s1, s2, map) => {
    console.log(`${roomname} : 게임 시작!`)
    Rooms[roomname].isPlaying = true;
    socket.to(roomname).emit('GameStart2', s1, s2, map)


    for (let index = 0; index < Rooms[roomname].seatInfos.length; index++) {

      if (Rooms[roomname].seatInfos[index].Idx == 1) {
        Rooms[roomname].seatInfos[index].Idx = 0;
      }

    }
    var roomcheck = {
      name: Rooms[roomname].name,
      currentP: Rooms[roomname].currentP,
      maxP: Rooms[roomname].maxP,
      seatInfos: Rooms[roomname].seatInfos,
      isPlaying: Rooms[roomname].isPlaying,
      mapIdx: Rooms[roomname].mapIdx
    }

    io.to(roomname).emit('SlotReset', roomcheck)





    RoomLobyInfoEmit()
  })



  socket.on('CreateRoomCheck', (roomName, nickname) => {
    if (roomName in Rooms) {
      //방이 있는지 없는지 확인
      console.log(" 방이름 겹침!")
      socket.emit('Warnning', "방이름이 이미 있습니다")
      //방생성 실패
    }
    else {
      //방생성 성공
      socket.leave("Loby");
      socket.join(roomName);
      //들어갑니다.

      Users[socket.id].Room = roomName


      Rooms[roomName] = {
        name: roomName,
        currentP: 1,
        maxP: 4,
        seatInfos: [
          {
            seatname: nickname,
            characterIdx: 0,
            Idx: 2
          },
          {
            seatname: "",
            characterIdx: 0,
            Idx: 0
          },
          {
            seatname: "",
            characterIdx: 0,
            Idx: 0
          },
          {
            seatname: "",
            characterIdx: 0,
            Idx: 0
          },
        ],
        isPlaying: false,
        mapIdx: 0
      }

      console.log(`${roomName} : 방생성 성공`)

      socket.emit('CreateRoom')
      //성공했다고 이벤트를 보냅니다.
      RoomLobyInfoEmit()
      //방 목록을 전부 보내는 이벤트를 실행합니다.
    }

  })
  socket.on('RoomLeave', (roomname, idx) => {
    //방을 나갑니다

    socket.leave(roomname);
    socket.join('Loby');
    Users[socket.id].Room = "";
    //leave를 사용합니다.
    console.log(`${Users[socket.id].loginID} : 방 나감`)

    if (Rooms[roomname].currentP <= 1) {
      //현재 방인원이 1이라면 삭제를 시킵니다.
      delete Rooms[roomname]
    }
    else {


      if (Rooms[roomname].seatInfos[idx].Idx == 2) {
        Rooms[roomname].seatInfos[idx].seatname = "";
        Rooms[roomname].seatInfos[idx].characterIdx = 0;
        Rooms[roomname].seatInfos[idx].Idx = 0;
        Rooms[roomname].currentP--;

        idx = 0;
        for (var index = 0; index < Rooms[roomname].seatInfos.length; index++) {
          if (Rooms[roomname].seatInfos[index].seatname != "" && Rooms[roomname].seatInfos[index].seatname != "막음") {
            idx = index;
            break;
          }

        }
        Rooms[roomname].seatInfos[idx].Idx = 2;


      }
      else {
        Rooms[roomname].seatInfos[idx].seatname = "";
        Rooms[roomname].seatInfos[idx].characterIdx = 0;
        Rooms[roomname].seatInfos[idx].Idx = 0;
        Rooms[roomname].currentP--;
      }

      var roomcheck = {
        name: Rooms[roomname].name,
        currentP: Rooms[roomname].currentP,
        maxP: Rooms[roomname].maxP,
        seatInfos: Rooms[roomname].seatInfos,
        isPlaying: Rooms[roomname].isPlaying,
        mapIdx: Rooms[roomname].mapIdx

      }
      socket.to(roomname).emit('SlotReset', roomcheck)


    }
    RoomLobyInfoEmit()
  })


  socket.on('LobyChat', (nick, text) => {
    //채팅을 보냅니다.

    console.log(`[Loby] : ${nick} : ${text}`)
    socket.to('Loby').emit('LobyChatGet', nick, text)
    //보인을 제외한 방에 존재한 사람들에게 보냅니다.

  })
  socket.on('RoomChat', (nick, text, roomname) => {
    //채팅을 보냅니다.


    console.log(`[Room] : ${nick} : ${text}`)
    socket.to(roomname).emit('RoomChatGet', nick, text)
    //보인을 제외한 방에 존재한 사람들에게 보냅니다.

  })
  console.log("연결함 : " + socket.id);


  socket.on('GameWait', (roomname) => {

    io.to(roomname).emit('GameWait')


  })
  socket.on('PlayChat', (roomname, id, s, playerIdx) => {
    console.log(`[Play] : ${id} : ${s}`)
    socket.to(roomname).emit('PlayChat', id, s, playerIdx)



  })
  socket.on('Playmove', (roomname, playerIdx, s) => {

    socket.to(roomname).emit('Playmove', playerIdx, s)


  })

  socket.on('PlayDead', (roomname, playerIdx) => {
    console.log(`${roomname} : 죽음`)
    socket.to(roomname).emit('PlayDead', playerIdx)


  })

  socket.on('Bomb', (roomname, data0, data1, data2, data3, data4) => {
    console.log(`${roomname} : 폭탄 놓음`)
    socket.to(roomname).emit('Bomb', data0, data1, data2, data3, data4)


  })
  socket.on('BrickMove', (roomname, wall_x, wall_y,dir_x,dir_y, playerIdx) => {
    console.log(`${roomname} : 벽 이동`)
    socket.to(roomname).emit('BrickMove', playerIdx,wall_x,wall_y,dir_x,dir_y)


  })

  socket.on('CharacterCreate', (roomname, playerIdx, characterIdx) => {

    console.log(`${roomname} : 캐릭 생성`)
    socket.to(roomname).emit('CharacterCreate', playerIdx, characterIdx)


  })
  socket.on('PlayEnd', (roomname) => {
    console.log(`${roomname} : 게임 종료`)
    if (roomname in Rooms) {
      Rooms[roomname].isPlaying = false;
    }
    RoomLobyInfoEmit()

  })
  socket.on('ItemRemove', (roomname, idx) => {
    socket.to(roomname).emit('ItemRemove', idx)
  })


  socket.on('record', (s, id, cnt,) => {

    if (s == "victory") {
      sql.query('UPDATE users SET victory=? WHERE ID=? ', [cnt, id], function (error, results) {
        {
          if (error) {
            console.log(error)
          }
          else {
            Users[socket.id].victory = cnt
            console.log(`${id} : 승리!`)
          }
        }
      })
    }
    else if (s == "defeat") {
      sql.query('UPDATE users SET defeat=? WHERE ID=? ', [cnt, id], function (error, results) {
        {
          if (error) {
            console.log(error)
          }
          else {
            Users[socket.id].defeat = cnt
            console.log(`${id} : 패배!`)
          }
        }
      })
    }
    else if (s == "draw") {
      sql.query('UPDATE users SET draw=? WHERE ID=? ', [cnt, id], function (error, results) {
        {
          if (error) {
            console.log(error)
          }
          else {
            Users[socket.id].draw = cnt
            console.log(`${id} : 무승부!`)
          }
        }
      })
    }

  })


  socket.on('disconnect', zz => {
    console.log("연결끊김 : " + socket.id);

    if (Users[socket.id].Room != "") {

      var roomname = Users[socket.id].Room;


      if (Rooms[roomname].currentP <= 1) {
        //현재 방인원이 1이라면 삭제를 시킵니다.
        delete Rooms[roomname]
        console.log(`강제종료 방삭제`)
      }
      else {
        var name = Users[socket.id].loginID;
        console.log(`${name} : 강제종료 방인원 조정`)

        var idx = 0;

        for (var index = 0; index < Rooms[roomname].seatInfos.length; index++) {
          if (Rooms[roomname].seatInfos[index].seatname == name) {
            idx = index;
            break;
          }

        }

        if (Rooms[roomname].seatInfos[idx].Idx == 2) {
          Rooms[roomname].seatInfos[idx].seatname = "";
          Rooms[roomname].seatInfos[idx].characterIdx = 0;
          Rooms[roomname].seatInfos[idx].Idx = 0;
          Rooms[roomname].currentP--;

          idx = 0;
          for (var index = 0; index < Rooms[roomname].seatInfos.length; index++) {
            if (Rooms[roomname].seatInfos[index].seatname != "" && Rooms[roomname].seatInfos[index].seatname != "막음") {
              idx = index;
              break;
            }

          }
          Rooms[roomname].seatInfos[idx].Idx = 2;


        }
        else {
          Rooms[roomname].seatInfos[idx].seatname = "";
          Rooms[roomname].seatInfos[idx].characterIdx = 0;
          Rooms[roomname].seatInfos[idx].Idx = 0;
          Rooms[roomname].currentP--;
        }


        var roomcheck = {
          name: Rooms[roomname].name,
          currentP: Rooms[roomname].currentP,
          maxP: Rooms[roomname].maxP,
          seatInfos: Rooms[roomname].seatInfos,
          isPlaying: Rooms[roomname].isPlaying,
          mapIdx: Rooms[roomname].mapIdx

        }
        socket.to(roomname).emit('SlotReset', roomcheck)
        if (Rooms[roomname].isPlaying) {


          sql.query('UPDATE users SET defeat=? WHERE ID=? ', [Users[socket.id].defeat + 1, name], function (error, results) {
            {
              if (error) {
                console.log(error)
              }
              else {
                console.log(`${name} : 플레이중 종료 패배 올라감`)
              }
            }
          })


          socket.to(roomname).emit('PlayerExit', name)
        }


      }
      RoomLobyInfoEmit()
    }




    delete Users[socket.id]

    //유저 정보 삭제
  })



});




server.listen(port, () => {
  console.log('listening on *:' + port);
});


