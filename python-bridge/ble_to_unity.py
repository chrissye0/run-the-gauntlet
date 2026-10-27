
import asyncio
import socket
import struct

from bleak import BleakClient, BleakScanner

CHARACTERISTIC_UUID = (
    "0000fff1-0000-1000-8000-00805f9b34fb"
)

udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# decode and forward to unity
def on_notification(hand, sender, data):
    # check to see if correct length
    if len(data) != 12:
        print(
            f"[{hand}] Unexpected packet size: "
            f"{len(data)} bytes"
        )
        return
    # six signed 16-bit integers
    raw = struct.unpack("<6h", data)
    # convert to sensor units
    ax, ay, az = [v / 1000.0 for v in raw[:3]]
    gx, gy, gz = [v / 10.0 for v in raw[3:]]
    # start with L or R to identify hand
    message = (
        f"{hand},{ax:.3f},{ay:.3f},{az:.3f},"
        f"{gx:.1f},{gy:.1f},{gz:.1f}"
    )
    udp.sendto(
        message.encode("utf-8"),
        ("127.0.0.1", 5005)
    )
    print(f"[{hand}] Forwarded: {message}")

# maintain BLE connection
async def connect_and_stream(device, hand):
    print(f"[{hand}] Connecting to {device.name}")
    async with BleakClient(device) as client:
        print(f"[{hand}] Connected.")
        # capture in callback
        def notification_handler(sender, data):
            on_notification(hand, sender, data)
        await client.start_notify(
            CHARACTERISTIC_UUID,
            notification_handler
        )
        print(f"[{hand}] Receiving IMU notifications")
        while True:
            await asyncio.sleep(1)

async def main():
    print("Scanning for both XIAOs")
    discovered = await BleakScanner.discover(
        timeout=20.0,
        return_adv=True
    )
    devices_by_name = {}
    for address, (device, advertisement) in discovered.items():
        name = advertisement.local_name or device.name
        if name:
            print(f"Found: {name} ({address})")
            devices_by_name[name] = device
    # get devices by their names
    left = devices_by_name.get("XIAO_LEFT")
    right = devices_by_name.get("XIAO_RIGHT")
    # see if anything is missing
    missing = []
    if left is None:
        missing.append("XIAO_LEFT")
    if right is None:
        missing.append("XIAO_RIGHT")
    if missing:
        print("Could not find: " + ", ".join(missing))
        print("Check power, advertising names, and phone connections.")
        return
    print("Both devices found")
    # run both BLE connections concurrently
    await asyncio.gather(
        connect_and_stream(left, "L"),
        connect_and_stream(right, "R")
    )

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\nBLE bridge stopped")
    except Exception as exc:
        print(f"BLE bridge error: {exc}")
    finally:
        udp.close()