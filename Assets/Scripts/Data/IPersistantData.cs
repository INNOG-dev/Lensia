using MySql.Data.MySqlClient;

public interface IPersistantData 
{

    public void saveData(MysqlHandler handler);

    public void loadData(MysqlHandler handler);


}
