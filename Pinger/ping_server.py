import sqlite3
from datetime import datetime

import yaml
from yaml import Loader
from mcstatus import JavaServer


def load_config():
    with open("config.yaml", "r") as f:
        data = f.readlines()
    options = yaml.load("".join(data), Loader=Loader)
    return options


def ping_server(ip, port):
    try:
        server = JavaServer(ip, port)
        status = server.status()

        player_info = [{'name': player.name, 'id': player.id} for player in status.players.sample]
        return player_info
    except:
        return 0


def update_database(data, db_connection):
    update_players(data, db_connection)
    update_player_sessions(data, db_connection)


def update_players(data, db_connection):
    conn = sqlite3.connect(db_connection)
    conn.set_trace_callback(print)
    c = conn.cursor()

    c.execute("UPDATE players SET last_online=CURRENT_TIMESTAMP WHERE last_online = 'now'")

    for player in data:
        c.execute("SELECT * FROM players WHERE id=?", (player['id'],))
        if c.fetchone() is None:
            c.execute(
                "INSERT INTO players (id, name, last_online, total_play_time) VALUES (?, ?, ?, 60)",
                (player['id'], player['name'], 'now'))
        else:
            c.execute("UPDATE players SET last_online=?, total_play_time=total_play_time+60, name=? WHERE id=?",
                      ('now', player['name'], player['id']))

    conn.commit()
    conn.close()


def update_player_sessions(data, db_connection):
    conn = sqlite3.connect(db_connection)
    conn.set_trace_callback(print)
    c = conn.cursor()

    date = datetime.now().strftime("%Y-%m-%d")
    for player in data:
        c.execute("SELECT * FROM player_sessions WHERE player_id=? AND date=?", (player['id'], date))
        if c.fetchone() is None:
            c.execute("INSERT INTO player_sessions (player_id, date, time_played_in_session) VALUES (?, ?, 60)",
                      (player['id'], date))
        else:
            c.execute(
                "UPDATE player_sessions SET time_played_in_session=time_played_in_session+60 WHERE player_id=? AND date=?",
                (player['id'], date))

    conn.commit()
    conn.close()


if __name__ == "__main__":
    config = load_config()
    ping_data = ping_server(config['server'], config['port'])
    update_database(ping_data, config['db_connection_string'])
