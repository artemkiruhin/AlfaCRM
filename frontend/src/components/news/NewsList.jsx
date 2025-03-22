import {ChevronRight} from "lucide-react";
import React from "react";
import {formatDate} from "../../extensions/utils";
import NewsPost from "./NewsPost";

const NewsList = ({newsItems, handleNewsClick}) => {
    return (
        <div className="news-grid">
            {newsItems.map(news => (
                <NewsPost key={news.id} post={news} handleNewsClick={handleNewsClick} />
            ))}
        </div>
    )
}

export default NewsList;