package main

import (
	"context"
	"fmt"
	"time"

	"github.com/philippseith/signalr"
)

var (
	serial string
	stream bool
)

type camera struct {
	signalr.Receiver
}

func (c *camera) StopStream() {
	fmt.Println("stopping stream")
	//c.Server().Send("stopping")
	stream = false
}

func (c *camera) InitDevice() {
	c.Server().Invoke("ReceiveInit", "Initialized Connection")
}

func (c *camera) StartStream(goodorbad string, side string) {
	fmt.Println("StartStream", goodorbad)
	stream = true
	ch := make(chan string, 1)
	method := "ReceiveGoodStream"
	if goodorbad == "bad" {
		method = "ReceiveBadStream"
	}
	c.Server().PushStreams(method, ch)
	go func(ch chan string) {
		count := 1
		for stream {
			if side == "left" {
				side = "left "
			}
			ch <- fmt.Sprintln(side, "Streaming Item", count)
			count += 1
			time.Sleep(1 * time.Second)
		}
		ch <- fmt.Sprintln(side, "Stream stopped")
	}(ch)
}

func runClient() error {
	serial = "abcdefg"
	stream = false

	//var ctx context.Context
	ctx, _ := context.WithCancel(context.Background())
	//defer cancelClient()
	creationCtx, _ := context.WithTimeout(context.Background(), 2*time.Second)
	conn, err := signalr.NewHTTPConnection(creationCtx, "https://localhost:7124/stream")

	if err != nil {
		fmt.Println("ERROR: ", err)
		return err
	}
	fmt.Println("conn is ", conn)
	client, err := signalr.NewClient(ctx, signalr.WithConnection(conn), signalr.WithReceiver(&camera{}), signalr.TransferFormat("Text"))
	if err != nil {
		fmt.Println("ERROR2: ", err)
		return err
	}
	client.Start()
	result := <-client.Invoke("AddDevice", serial)
	fmt.Println("result is ", result)
	//client.WaitForState(context.Background(), signalr.ClientClosed)
	return nil
}

func main() {
	go func() {
		runClient()
	}()
	ch := make(chan struct{})
	<-ch
}
