using StudentTracker.Domain.Common;

namespace StudentTracker.Domain.Entities;

//Класс для создания учеб дисцплины
    
public class Subject:BaseEntity
{
    public string Name { get; private set; }

    public string Description { get; private set; }

    public Subject(string name, string? description)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject name is required", nameof(name));
        
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        

    }
    //метод Update нужен для изменения дисцплины 
    public void  Update(string name, string description)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject description is required", nameof(name));
        
        if(string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Subject description is required", nameof(description));
        Name = name ;
        Description = description;  

    }
}
