using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrelloNet;
using TrelloNet.Extensions;

namespace com.bemaservices.TrelloSync.Model
{
    public class Comments
    {
        public List<Board> Boards { get; set; }
        public List<CardComments> Cards { get; set; }
        public List<Comment> Actions { get; set; }
    }

    public class Board
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ShortLink { get; set; }
        public List<CardComments> Cards { get; set; }
        public List<List> Lists { get; set; }

    }

    public class List
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BoardId { get; set; }
        public List<Card> Cards { get; set; }
    }

    public class CardComments
    {

        public Card Card { get; set; }
        public List<Comment> Comments { get; set; }

    }

    public class Comment
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string CardId { get; set; }


    }
}
